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
using Telegram.Bot.Types.ReplyMarkups;

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

            var args = message.ParseMessage("/new_game");
            var request = new CreateTelegramGameRequest();
            if (!request.InitFromArgs(args))
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

            var newMessage = await botClient.SendTextMessageAsync(message.Chat.Id, gameMessage, parseMode: ParseMode.Markdown, replyMarkup: GetGameReplyMarkup());

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
        public async Task<Message> UpdateGameAsync(Message message)
        {
            try
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }

            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var chatId = message.Chat.Id;

            var findTelegramGameRequest = new FindTelegramGameRequest
            {
                ChatId = chatId.ToString(),
                MessageId = message.ReplyToMessage?.MessageId.ToString()
            };

            var telegramGame = await apiService.FindTelegramGameAsync(findTelegramGameRequest);
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
                telegramUser = await CreateTelegramUserAsync(message.From, chatId.ToString());
                if (telegramUser == null)
                {
                    logger.LogError($"Telegram user '{message.From.FirstName}' not found");
                    return null;
                }
            }

            var telegramChatUser = await apiService.FindTelegramChatUserAsync(new FindTelegramChatUserRequest
            {
                TelegramChatId = chatId.ToString(),
                TelegramUserId = telegramUser.Id
            });

            if (telegramChatUser == null)
            {
                var createChatUserRequest = new CreateTelegramChatUserRequest
                {
                    TelegramChatId = chatId.ToString(),
                    TelegramUserId = telegramUser.Id,
                    IsAdmin = await IsAdminAsync(chatId.ToString(), message.From.Id)
                };

                telegramChatUser = await apiService.CreateTelegramChatUserAsync(createChatUserRequest);
            }

            if (telegramChatUser == null || !telegramChatUser.IsAdmin)
            {
                logger.LogError($"Telegram user 'Id = {telegramUser.TelegramId}, Name = {telegramUser.FirstName} {telegramUser.LastName}' isn't admin in chat {message.Chat.Id}");
                return null;
            }

            var args = message.ParseMessage("/update_game");
            var request = new UpdateTelegramGameRequest()
            {
                ChatId = chatId.ToString(),
                MessageId = message.ReplyToMessage?.MessageId.ToString()
            };

            if (!request.InitFromArgs(args))
            {
                logger.LogError($"Message {message.Text} cannot to convert to update game request");
                return null;
            }

            telegramGame = await apiService.UpdateTelegramGameAsync(request);
            if (telegramGame == null)
            {
                logger.LogError($"Telegram game didn't update");
                return null;
            }

            var gameMessage = PlayersChangedGenerateGameMessage(telegramGame);
            var newMessage = await botClient.EditMessageTextAsync(message.Chat.Id, message.ReplyToMessage.MessageId, gameMessage, parseMode: ParseMode.Markdown, replyMarkup: GetGameReplyMarkup());
            return newMessage;
        }
        public async Task<Message> CreateGamePlayerAsync(Message message, User from, Message replyMessage = null)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var chatId = message.Chat.Id;

            var request = new FindTelegramGameRequest
            {
                ChatId = chatId.ToString()
            };

            if(replyMessage == null)
            {
                request.MessageId = message.MessageId.ToString();
            }
            else
            {
                request.MessageId = replyMessage.MessageId.ToString();
            }

            var telegramGame = await apiService.FindTelegramGameAsync(request);
            if (telegramGame == null)
            {
                logger.LogError($"Telegram game didn't found");
                return null;
            }

            if (telegramGame.Date < DateTime.UtcNow)
            {
                logger.LogWarning($"Telegram game isn't active");

                if (replyMessage == null)
                {
                    return null;
                }

                return await botClient.SendTextMessageAsync(message.Chat.Id, $"Почекай наступну гру, ця вже відбулась(", replyToMessageId: message.MessageId);
            }

            var telegramUser = await apiService.FindTelegramUserAsync(new FindTelegramUserRequest { TelegramId = from.Id.ToString() });
            if (telegramUser == null)
            {
                telegramUser = await CreateTelegramUserAsync(from, chatId.ToString());
                if (telegramUser == null)
                {
                    logger.LogError($"Telegram user '{from.FirstName}' not found");
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

            var name = $"{telegramUser.FirstName} {telegramUser.LastName}";
            var maxPlayersCount = GetMaxGamePlayersCount(telegramGame.TelegramGamePlayers.Count);
            if (telegramGame.TelegramGamePlayers.Count > maxPlayersCount)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"| [{name}](tg://user?id={telegramUser.TelegramId}) + | Додав в чергу очікування", parseMode: ParseMode.Markdown);
            }
            else
            {
                var gamePlayersCount = maxPlayersCount - telegramGame.TelegramGamePlayers.Count;
                if (gamePlayersCount > 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"| [{name}](tg://user?id={telegramUser.TelegramId}) + | Продовжуємо набір! Для гри потрібно ще мінімум *{maxPlayersCount - telegramGame.TelegramGamePlayers.Count}* гравців", parseMode: ParseMode.Markdown);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"| [{name}](tg://user?id={telegramUser.TelegramId}) + | *Гра відбудеться! Нас {maxPlayersCount}*", parseMode: ParseMode.Markdown);
                }
            }

            var gameMessage = PlayersChangedGenerateGameMessage(telegramGame);

            Message newMessage;
            if (replyMessage == null)
            {
                newMessage = await botClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, gameMessage, parseMode: ParseMode.Markdown, replyMarkup: GetGameReplyMarkup());
            }
            else
            {
                newMessage = await botClient.EditMessageTextAsync(message.Chat.Id, replyMessage.MessageId, gameMessage, parseMode: ParseMode.Markdown, replyMarkup: GetGameReplyMarkup());
            }

            if (replyMessage != null)
            {
                try
                {
                    await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                }
            }
            
            return newMessage;
        }
        public async Task<Message> DeleteGamePlayerAsync(Message message, User from, Message replyMessage = null)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var chatId = message.Chat.Id;

            var request = new FindTelegramGameRequest
            {
                ChatId = chatId.ToString(),
            };

            if (replyMessage == null)
            {
                request.MessageId = message.MessageId.ToString();
            }
            else
            {
                request.MessageId = replyMessage.MessageId.ToString();
            }

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

            var telegramUser = await apiService.FindTelegramUserAsync(new FindTelegramUserRequest { TelegramId = from.Id.ToString() });
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

                var name = $"{telegramGamePlayer.TelegramUser.FirstName} {telegramGamePlayer.TelegramUser.LastName}";
                if (deletedUserIndex + 1 <= maxPlayersCount && telegramGame.TelegramGamePlayers.Count >= maxPlayersCount)
                {
                    var newMainUser = gameUserList[maxPlayersCount - 1];
                    var newUserName = $"{newMainUser.TelegramUser.FirstName} {newMainUser.TelegramUser.LastName}";
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"| [{name}](tg://user?id={telegramUser.TelegramId}) - | [{newUserName}](tg://user?id={newMainUser.TelegramUser.TelegramId}), ти у грі", parseMode: ParseMode.Markdown);
                }

                var newMaxPlayersCount = GetMaxGamePlayersCount(telegramGame.TelegramGamePlayers.Count);
                var gamePlayersCount = newMaxPlayersCount - telegramGame.TelegramGamePlayers.Count;
                if (gamePlayersCount > 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"| [{name}](tg://user?id={telegramUser.TelegramId}) - | Продовжуємо набір! Для гри потрібно ще мінімум *{newMaxPlayersCount - telegramGame.TelegramGamePlayers.Count}* гравців", parseMode: ParseMode.Markdown);
                }
                else if(newMaxPlayersCount < maxPlayersCount)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"| [{name}](tg://user?id={telegramUser.TelegramId}) - | *Гра відбудеться! Нас {newMaxPlayersCount}*", parseMode: ParseMode.Markdown);
                }
            }

            var gameMessage = PlayersChangedGenerateGameMessage(telegramGame);

            Message newMessage;
            if (replyMessage == null)
            {
                newMessage = await botClient.EditMessageTextAsync(message.Chat.Id, message.MessageId, gameMessage, parseMode: ParseMode.Markdown, replyMarkup: GetGameReplyMarkup());
            }
            else
            {
                newMessage = await botClient.EditMessageTextAsync(message.Chat.Id, replyMessage.MessageId, gameMessage, parseMode: ParseMode.Markdown, replyMarkup: GetGameReplyMarkup());
            }
            
            if (replyMessage != null)
            {
                try
                {
                    await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                }
            }

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
        //public async Task<Message> GenerateGameTeamsAsync(Message message)
        //{
        //    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        //    var chatId = message.Chat.Id;

        //    var request = new FindTelegramGameRequest
        //    {
        //        ChatId = chatId.ToString(),
        //        MessageId = message.ReplyToMessage?.MessageId.ToString()
        //    };

        //    var telegramGame = await apiService.FindTelegramGameAsync(request);
        //    if (telegramGame == null)
        //    {
        //        logger.LogError($"Telegram game didn't found");
        //        return null;
        //    }

        //    if (telegramGame.Date < DateTime.UtcNow)
        //    {
        //        logger.LogWarning($"Telegram game isn't active");
        //        return await botClient.SendTextMessageAsync(message.Chat.Id, $"Цю гру вже зіграли!", replyToMessageId: message.MessageId);
        //    }

        //    if (telegramGame.TelegramGamePlayers == null || telegramGame.TelegramGamePlayers.Count < 10)
        //    {
        //        logger.LogWarning($"Telegram game isn't full");
        //        return await botClient.SendTextMessageAsync(message.Chat.Id, $"У команді менше 10 гравців!", replyToMessageId: message.MessageId);
        //    }

        //    var gameTeams = await CreateTelegramGameTeamsAsync(message.Chat.Id.ToString(), telegramGame.TelegramGamePlayers);
        //    var gameTeamsMessage = GetTelegramGameTeamsMessage(gameTeams);
        //    var newMessage = await botClient.SendTextMessageAsync(message.Chat.Id, gameTeamsMessage);

        //    try
        //    {
        //        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex.Message, ex);
        //    }

        //    return newMessage;
        //}

        private InlineKeyboardMarkup GetGameReplyMarkup() => new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("+"),
                InlineKeyboardButton.WithCallbackData("-")
            }
        });
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
            StringBuilder gameUsers;
            StringBuilder reserveUsers;

            if(lastGame == null)
            {
                gameUsers = new StringBuilder();
                reserveUsers = new StringBuilder();
            }
            else
            {
                FillUsers(out gameUsers, out reserveUsers, lastGame.TelegramGamePlayers);
            }

            var address = request.Address;
            if(!string.IsNullOrEmpty(request.Notes))
            {
                address += $" ({request.Notes})";
            }

            return string.Format(
                Constants.GameMessageFormat,
                request.Date,
                request.DurationInMins,
                request.Cost,
                address,
                gameUsers.ToString(),
                reserveUsers.ToString());
        }
        private string PlayersChangedGenerateGameMessage(TelegramGame game)
        {
            StringBuilder gameUsers;
            StringBuilder reserveUsers;

            FillUsers(out gameUsers, out reserveUsers, game.TelegramGamePlayers);


            var address = game.Address;
            if (!string.IsNullOrEmpty(game.Notes))
            {
                address += $" ({game.Notes})";
            }

            return string.Format(
                Constants.GameMessageFormat,
                game.Date,
                game.DurationInMins,
                game.Cost,
                address,
                gameUsers.ToString(),
                reserveUsers.ToString());
        }
        private void FillUsers(out StringBuilder gameUsers, out StringBuilder reserveUsers, ICollection<TelegramGamePlayer> telegramGamePlayers)
        {
            gameUsers = new StringBuilder();
            reserveUsers = new StringBuilder();

            var counter = 0;
            var maxPlayersCount = GetMaxGamePlayersCount(telegramGamePlayers.Count);
            var userList = new List<TelegramGamePlayer>();

            foreach (var player in telegramGamePlayers.OrderBy(x => x.CreatedAt))
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
        }
        private int GetMaxGamePlayersCount(int playersCount)
        {
            if (playersCount <= 10)
            {
                return 10;
            }

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
        private int GetMaxTeamsCount(int playersCount)
        {
            var maxPlayersCount = GetMaxGamePlayersCount(playersCount);
            return maxPlayersCount switch
            {
                12 => 2,
                15 => 3,
                20 => 4,
                _ => 2
            };
        }
        private int GetPlayersPerTeamCount(int playersCount)
        {
            var maxPlayersCount = GetMaxGamePlayersCount(playersCount);
            return maxPlayersCount switch
            {
                10 => 5,
                12 => 6,
                15 => 5,
                18 => 6,
                20 => 5,
                _ => throw new ArgumentException("Wrong number of game players!")
            };
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
        //private async Task<ICollection<TelegramGameTeam>> CreateTelegramGameTeamsAsync(string gameId, ICollection<TelegramGamePlayer> telegramGamePlayers)
        //{
        //    var availableTeamNames = new string[] { "A", "B", "C", "D" };

        //    var players = telegramGamePlayers.OrderByDescending(x => x.TelegramUser.TelegramUserScore.Score);
        //    var maxPlayersCount = GetMaxGamePlayersCount(telegramGamePlayers.Count);

        //    var gamePlayers = telegramGamePlayers
        //        .OrderBy(x => x.CreatedAt)
        //        .Take(maxPlayersCount)
        //        .ToList();

        //    var maxTeamsCount = GetMaxTeamsCount(maxPlayersCount);
        //    var playersPerTeams = GetPlayersPerTeamCount(maxPlayersCount);

        //    var teams = new Dictionary<string, ICollection<TelegramGamePlayer>>();

        //    for(int i = 0; i < maxTeamsCount; i++)
        //    {
        //        teams.Add(availableTeamNames[i], new List<TelegramGamePlayer>());
        //    }

        //    //var moreSkillPlayers = players.Take(telegramGamePlayers.Count / 2).ToList();
        //    //var lessSkillPlayers = players.TakeLast(telegramGamePlayers.Count - moreSkillPlayers.Count).ToList();

        //    //var moreSkillPlayersGroup = moreSkillPlayers.GroupBy(x => x.TelegramUser.TelegramUserScore.Score);
        //    //var lessSkillPlayersGroup = lessSkillPlayers.GroupBy(x => x.TelegramUser.TelegramUserScore.Score);

        //    var processedPlayers = 0;

        //    while(processedPlayers <= maxPlayersCount)
        //    {
        //        var minScore = teams.Min(x => x.Value.Sum(y => y.TelegramUser.TelegramUserScore.Score));
        //        var minTeamsScore = teams
        //            .Where(x => x.Value.Count < playersPerTeams && x.Value.Sum(y => y.TelegramUser.TelegramUserScore.Score) <= minScore)
        //            .ToList();

        //        var minTeamIndex = new Random().Next(0, minTeamsScore.Count);

        //        minTeamsScore[minTeamIndex].Value.Add(gamePlayers[processedPlayers]);
        //        processedPlayers++;
        //    }

        //    //TODO: create telegram game team

        //    return null;
        //}
        //private string GetTelegramGameTeamsMessage(ICollection<TelegramGameTeam> teams)
        //{
        //    return null;
        //}
         
    }
}
