using Football.Collector.Telegram.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Football.Collector.Telegram.Services
{
    public class HandleUpdateService
    {
        private readonly IGameService gameService;
        private readonly ILogger<HandleUpdateService> logger;

        public HandleUpdateService(ILogger<HandleUpdateService> logger, IGameService gameService)
        {
            this.gameService = gameService;
            this.logger = logger;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            try
            {
                var handler = update.Type switch
                {
                    UpdateType.Message => HandleMessageUpdate(update.Message),
                    UpdateType.MyChatMember => OnChatMemberChanged(update.MyChatMember),
                    UpdateType.ChatMember => OnChatMemberChanged(update.ChatMember),
                    _ => UnknownUpdateHandlerAsync(update)
                };

                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }
        public Task HandleMessageUpdate(Message message)
        {
            return message.Type switch
            {
                MessageType.ChatMembersAdded => OnChatMemberAdded(message.Chat, message.NewChatMembers),
                MessageType.Text => OnTextMessageReceived(message),
                _ => Task.CompletedTask
            };
        }
        public Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            logger.LogInformation(ErrorMessage);
            return Task.CompletedTask;
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            logger.LogInformation($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
        private async Task OnTextMessageReceived(Message message)
        {
            logger.LogInformation($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
            {
                return;
            }

            var action = message.Text.Split(' ').First() switch
            {
               "/new_game" => gameService.CreateGameAsync(message),
                "+" => gameService.CreateGamePlayerAsync(message),
                "-" => gameService.DeleteGamePlayerAsync(message),
                _ => NoActionAsync(message)
            };

            var sentMessage = await action;

            if(sentMessage != null && sentMessage.MessageId != 0)
            {
                logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");
            }
        }
        private async Task OnChatMemberAdded(Chat chat, params User[] newUsers)
        {
            foreach (var newUser in newUsers)
            {
                logger.LogInformation($"Receive chat member added event. Id = {newUser.Id}, Name = {newUser.FirstName} {newUser.LastName}, Username = {newUser.Username}");

                if (newUser.IsBot)
                {
                    logger.LogInformation($"Bot ignored");
                    return;
                }

                var sentMessage = await gameService.AddNewChatUserAsync(chat, newUser);
                if (sentMessage != null && sentMessage.MessageId != 0)
                {
                    logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");
                }
            }
        }
        private async Task OnChatMemberChanged(ChatMemberUpdated chatMemberUpdated)
        {
            logger.LogInformation($"Receive chat member update. Old status: {chatMemberUpdated.OldChatMember.Status}, New Status: {chatMemberUpdated.NewChatMember.Status}");
            
            if(chatMemberUpdated.NewChatMember.Status == ChatMemberStatus.Kicked)
            {
                logger.LogInformation($"User {chatMemberUpdated.NewChatMember.User.Username} kicked from chat {chatMemberUpdated.Chat.Id}");
                return;
            }

            if (chatMemberUpdated.NewChatMember.Status == ChatMemberStatus.Member)
            {
                var sentMessage =  await gameService.AddNewChatUserAsync(chatMemberUpdated.Chat, chatMemberUpdated.NewChatMember.User);
                if (sentMessage != null && sentMessage.MessageId != 0)
                {
                    logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");
                }

                if (chatMemberUpdated.NewChatMember.User.IsBot)
                {
                    logger.LogInformation($"Bot {chatMemberUpdated.NewChatMember.User.Username} added to chat {chatMemberUpdated.Chat.Id}");
                }
                else
                {
                    logger.LogInformation($"User {chatMemberUpdated.NewChatMember.User.Username} joined to chat {chatMemberUpdated.Chat.Id}");
                }

                return;
            }
        }

        static Task<Message> NoActionAsync(Message message)
        {
            return Task.FromResult(new Message());
        }
    }
}
