using Football.Collector.Common.Models;
using Football.Collector.Data.Models;
using Football.Collector.Telegram.Extensions;
using Football.Collector.Telegram.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Football.Collector.Telegram.Services
{
    public class GameService : IGameService
    {
        private readonly ITelegramBotClient botClient;
        private readonly ILogger<GameService> logger;
        private readonly IApiService apiService;

        public GameService(ITelegramBotClient botClient, IApiService apiService, ILogger<GameService> logger)
        {
            this.botClient = botClient;
            this.apiService = apiService;
            this.logger = logger;
        }
        public async Task<Message> CreateGameAsync(Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var telegramUser = await apiService.FindTelegramUserAsync(new FindTelegramUserRequest { TelegramId = message.From.Id.ToString() });
            if (telegramUser == null)
            {
                telegramUser = await CreateTelegramUserAsync(message.From, message.Chat.Id.ToString());
                if (telegramUser == null)
                {
                    logger.LogError($"Telegram user '{message.From.FirstName}' not found");
                    return null;
                }
            }

            var telegramChatUser = await apiService.FindTelegramChatUserAsync(new FindTelegramChatUserRequest
            {
                TelegramChatId = message.Chat.Id.ToString(),
                TelegramUserId = telegramUser.Id
            });

            if (telegramChatUser == null)
            {
                var createChatUserRequest = new CreateTelegramChatUserRequest
                {
                    TelegramChatId = message.Chat.Id.ToString(),
                    TelegramUserId = telegramUser.Id,
                    IsAdmin = await IsAdminAsync(message.Chat.Id.ToString(), message.From.Id)
                };

                telegramChatUser = await apiService.CreateTelegramChatUserAsync(createChatUserRequest);
            }

            if (telegramChatUser == null || !telegramChatUser.IsAdmin)
            {
                logger.LogError($"Telegram user 'Id = {telegramUser.TelegramId}, Name = {telegramUser.FirstName} {telegramUser.LastName}' isn't admin in chat {message.Chat.Id}");
                return null;
            }

            var args = message.ParseMessage();
            var request = CreateTelegramGameRequest(args);
            if (request == null)
            {
                logger.LogError($"Message {message.Text} cannot to convert to create game request");
                return null;
            }

            TelegramGame lastGame = null;
            if(args.TryGetValue("includeLastGamePlayers", out var includeLastGamePlayers) && includeLastGamePlayers == "true")
            {
                lastGame = await apiService.FindLastTelegramGameAsync(new FindLastTelegramGameRequest 
                { 
                    Date = request.Date, 
                    TelegramChatId = telegramChatUser.TelegramChatId 
                });
            }
            
            var gameMessage = NewGameGenerateGameMessage(request, lastGame);

            var newMessage = await botClient.SendTextMessageAsync(message.Chat.Id, gameMessage, parseMode: ParseMode.Markdown);

            request.ChatId = newMessage.Chat.Id.ToString();
            request.MessageId = newMessage.MessageId.ToString();
            request.LastGameId = lastGame?.Id;

            var telegramGame = await apiService.CreateTelegramGameAsync(request);
            if (telegramGame == null)
            {
                logger.LogError($"Telegram game didn't create");
                await botClient.DeleteMessageAsync(newMessage.Chat.Id, newMessage.MessageId);
                return null;
            }

            await botClient.PinChatMessageAsync(message.Chat.Id, newMessage.MessageId);

            try
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }

            return newMessage;
        }
        public async Task<Message> CreateGamePlayerAsync(Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var chatId = message.Chat.Id;

            var request = new FindTelegramGameRequest
            {
                ChatId = chatId.ToString(),
                MessageId = message.ReplyToMessage?.MessageId.ToString()
            };

            var telegramGame = await apiService.FindTelegramGameAsync(request);
            if (telegramGame == null)
            {
                logger.LogError($"Telegram game didn't found");
                return null;
            }

            if (telegramGame.Date < DateTime.UtcNow)
            {
                logger.LogWarning($"Telegram game isn't active");
                return await botClient.SendTextMessageAsync(message.Chat.Id, $"Почекай наступну гру, ця вже відбулась(", replyToMessageId: message.MessageId);
            }

            var telegramUser = await apiService.FindTelegramUserAsync(new FindTelegramUserRequest { TelegramId = message.From.Id.ToString() });
            if (telegramUser == null)
            {
                telegramUser = await CreateTelegramUserAsync(message.From, chatId.ToString());
                if (telegramUser == null)
                {
                    logger.LogError($"Telegram user '{message.From.FirstName}' not found");
                    return null;
                }
            }

            var telegramGamePlayer = await apiService.CreateTelegramGamePlayerAsync(new CreateTelegramGamePlayerRequest 
            { 
                TelegramGameId = telegramGame.Id, 
                TelegramUserId = telegramUser.Id
            });
            if (telegramGamePlayer == null)
            {
                logger.LogError($"Game player didn't create");
                return null;
            }

            if(telegramGame.TelegramGamePlayers == null)
            {
                telegramGame.TelegramGamePlayers = new List<TelegramGamePlayer>();
            }

            telegramGamePlayer.TelegramUser = telegramUser;
            telegramGame.TelegramGamePlayers.Add(telegramGamePlayer);

            var maxPlayersCount = GetMaxGamePlayersCount(telegramGame.TelegramGamePlayers.Count);
            if (telegramGame.TelegramGamePlayers.Count > maxPlayersCount)
            {
                var name = $"{telegramUser.FirstName} {telegramUser.LastName}";
                await botClient.SendTextMessageAsync(message.Chat.Id, $"[{name}](tg://user?id={telegramUser.TelegramId}), додав тебе в чергу очікування", parseMode: ParseMode.Markdown);
            }
            else
            {
                var gamePlayersCount = maxPlayersCount - telegramGame.TelegramGamePlayers.Count;
                if (gamePlayersCount > 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Продовжуємо набір! Для гри потрібно ще мінімум {maxPlayersCount - telegramGame.TelegramGamePlayers.Count} гравців");
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Гра відбудеться! Нас {maxPlayersCount}");
                }
            }

            var gameMessage = PlayersChangedGenerateGameMessage(telegramGame);
            var newMessage = await botClient.EditMessageTextAsync(message.Chat.Id, message.ReplyToMessage.MessageId, gameMessage, parseMode: ParseMode.Markdown);
            return newMessage;
        }
        public async Task<Message> DeleteGamePlayerAsync(Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var chatId = message.Chat.Id;

            var request = new FindTelegramGameRequest
            {
                ChatId = chatId.ToString(),
                MessageId = message.ReplyToMessage?.MessageId.ToString()
            };

            var telegramGame = await apiService.FindTelegramGameAsync(request);
            if (telegramGame == null)
            {
                logger.LogError($"Telegram game didn't found");
                return null;
            }

            if (telegramGame.Date < DateTime.UtcNow)
            {
                logger.LogWarning($"Telegram game isn't active");
                return null;
            }

            var telegramUser = await apiService.FindTelegramUserAsync(new FindTelegramUserRequest { TelegramId = message.From.Id.ToString() });
            if (telegramUser == null)
            {
                logger.LogError($"Telegram user didn't found");
                return null;
            }

            var isSuccess = await apiService.DeleteTelegramGamePlayerAsync(new DeleteTelegramGamePlayerRequest 
            { 
                TelegramGameId = telegramGame.Id,
                TelegramUserId = telegramUser.Id 
            });

            if(!isSuccess)
            {
                return null;
            }

            var telegramGamePlayer = telegramGame.TelegramGamePlayers.FirstOrDefault(x => x.TelegramGameId == telegramGame.Id && x.TelegramUserId == telegramUser.Id);
            if (telegramGamePlayer != null)
            {
                var maxPlayersCount = GetMaxGamePlayersCount(telegramGame.TelegramGamePlayers.Count);

                var gameUserList = telegramGame.TelegramGamePlayers.OrderBy(x => x.CreatedAt).ToList();
                var deletedUserIndex = gameUserList.IndexOf(telegramGamePlayer);

                telegramGame.TelegramGamePlayers.Remove(telegramGamePlayer);
                gameUserList.Remove(telegramGamePlayer);

                if (deletedUserIndex + 1 <= maxPlayersCount && telegramGame.TelegramGamePlayers.Count >= maxPlayersCount)
                {
                    var newMainUser = gameUserList[maxPlayersCount - 1];
                    var name = $"{newMainUser.TelegramUser.FirstName} {newMainUser.TelegramUser.LastName}";
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"[{name}](tg://user?id={newMainUser.TelegramUser.TelegramId}), ти у грі", parseMode: ParseMode.Markdown);
                }

                var newMaxPlayersCount = GetMaxGamePlayersCount(telegramGame.TelegramGamePlayers.Count);
                var gamePlayersCount = newMaxPlayersCount - telegramGame.TelegramGamePlayers.Count;
                if (gamePlayersCount > 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Продовжуємо набір! Для гри потрібно ще мінімум {newMaxPlayersCount - telegramGame.TelegramGamePlayers.Count} гравців");
                }
                else if(newMaxPlayersCount < maxPlayersCount)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Гра відбудеться! Нас {newMaxPlayersCount}");
                }
            }

            var gameMessage = PlayersChangedGenerateGameMessage(telegramGame);
            var newMessage = await botClient.EditMessageTextAsync(message.Chat.Id, message.ReplyToMessage.MessageId, gameMessage, parseMode: ParseMode.Markdown);
            return newMessage;
        }
        public async Task<Message> AddNewChatUserAsync(Chat chat, User user)
        {
            await botClient.SendChatActionAsync(chat.Id, ChatAction.Typing);

            var telegramUser = await apiService.FindTelegramUserAsync(new FindTelegramUserRequest { TelegramId = user.Id.ToString() });
            if (telegramUser == null)
            {
                telegramUser = await CreateTelegramUserAsync(user, chat.Id.ToString());
            }

            if (telegramUser == null)
            {
                logger.LogError($"Telegram user didn't found");
                return null;
            }

            return await botClient.SendTextMessageAsync(chat.Id,
                string.Format(Constants.ChatRulesFormat, $"[{user.FirstName} {user.LastName}](tg://user?id={user.Id})"),
                parseMode: ParseMode.Markdown);
        }

        private async Task<TelegramUser> CreateTelegramUserAsync(User user, string chatId)
        {
            var request = new CreateTelegramUserRequest
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                TelegramId = user.Id.ToString(),
                Username = user.Username
            };

            var telegramUser = await apiService.CreateTelegramUserAsync(request);
            if (telegramUser == null)
            {
                return null;
            }

            var telegramChatUser = await apiService.FindTelegramChatUserAsync(new FindTelegramChatUserRequest
            {
                TelegramChatId = chatId,
                TelegramUserId = telegramUser.Id
            });

            if (telegramChatUser == null)
            {
                var createChatUserRequest = new CreateTelegramChatUserRequest
                {
                    TelegramChatId = chatId,
                    TelegramUserId = telegramUser.Id,
                    IsAdmin = await IsAdminAsync(chatId, user.Id)
                };

                telegramChatUser = await apiService.CreateTelegramChatUserAsync(createChatUserRequest);
            }

            return telegramUser;
        }
        private string NewGameGenerateGameMessage(CreateTelegramGameRequest request, TelegramGame lastGame)
        {
            var gameUsers = new StringBuilder();
            var reserveUsers = new StringBuilder();

            var counter = 0;
            if (lastGame != null)
            {
                var maxPlayersCount = GetMaxGamePlayersCount(lastGame.TelegramGamePlayers.Count);

                foreach (var player in lastGame.TelegramGamePlayers.OrderBy(x => x.CreatedAt))
                {
                    counter++;
                    if (counter <= maxPlayersCount)
                    {
                        gameUsers.AppendLine($"{counter}. [{player.TelegramUser.FirstName} {player.TelegramUser.LastName}](tg://user?id={player.TelegramUser.TelegramId})");
                    }
                    else
                    {
                        reserveUsers.AppendLine($"{counter}. {player.TelegramUser.FirstName} {player.TelegramUser.LastName}");
                    }
                }
            }

            return string.Format(
                Constants.GameMessageFormat,
                request.Date,
                request.DurationInMins,
                request.Cost,
                request.Address,
                gameUsers.ToString(),
                reserveUsers.ToString());
        }
        private string PlayersChangedGenerateGameMessage(TelegramGame game)
        {
            var gameUsers = new StringBuilder();
            var reserveUsers = new StringBuilder();

            var counter = 0;
            var maxPlayersCount = GetMaxGamePlayersCount(game.TelegramGamePlayers.Count);

            var userList = new List<TelegramGamePlayer>();

            foreach (var player in game.TelegramGamePlayers.OrderBy(x => x.CreatedAt))
            {
                counter++;

                var name = $"{player.TelegramUser.FirstName} {player.TelegramUser.LastName}";
                var duplicateCount = userList.Count(x => x.TelegramUserId == player.TelegramUserId);
                if (duplicateCount > 0)
                {
                    name += $"+{duplicateCount}";
                }

                userList.Add(player);

                if (counter <= maxPlayersCount)
                {
                    gameUsers.AppendLine($"{counter}. [{name}](tg://user?id={player.TelegramUser.TelegramId})");
                }
                else
                {
                    reserveUsers.AppendLine($"{counter}. {name}");
                }
            }

            return string.Format(
                Constants.GameMessageFormat,
                game.Date,
                game.DurationInMins,
                game.Cost,
                game.Address,
                gameUsers.ToString(),
                reserveUsers.ToString());
        }
        private int GetMaxGamePlayersCount(int playersCount)
        {
            if (playersCount <= 12)
            {
                return 12;
            }

            if (playersCount <= 15)
            {
                return 15;
            }

            if (playersCount <= 18)
            {
                return 18;
            }

            return 20;
        }
        private async Task<bool> IsAdminAsync(ChatId chatId, long userId)
        {
            try
            {
                var administrators = await botClient.GetChatAdministratorsAsync(chatId);
                if(administrators == null || administrators.Length == 0)
                {
                    return false;
                }

                return administrators.Any(x => x.User.Id == userId);
            }
            catch(Exception ex)
            {
                logger.LogError($"Exception occured while receiving administrators in chat: {chatId}. {ex.Message}");
                return false;
            }
        }
        private CreateTelegramGameRequest CreateTelegramGameRequest(IDictionary<string, string> args)
        {
            var request = new CreateTelegramGameRequest();
            foreach (var item in args)
            {
                switch (item.Key)
                {
                    case "address":
                        request.Address = item.Value;
                        break;

                    case "cost":
                        if (double.TryParse(item.Value, out var cost))
                        {
                            request.Cost = cost;
                        }
                        break;

                    case "date":
                        if (DateTime.TryParse(item.Value, out var date))
                        {
                            request.Date = date;
                        }
                        break;

                    case "durationInMins":
                        if (int.TryParse(item.Value, out var durationInMins))
                        {
                            request.DurationInMins = durationInMins;
                        }
                        break;
                }
            }

            return request;
        }
    }
}
