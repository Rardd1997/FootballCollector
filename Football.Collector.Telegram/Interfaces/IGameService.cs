using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Football.Collector.Telegram.Interfaces
{
    public interface IGameService
    {
        Task<Message> CreateGameAsync(Message message);

        Task<Message> UpdateGameAsync(Message message);

        Task<Message> CreateGamePlayerAsync(Message message, User from, Message replyMessage = null);

        Task<Message> DeleteGamePlayerAsync(Message message, User from, Message replyMessage = null);

        Task<Message> AddNewChatUserAsync(Chat chat, User user);

        Task<Message> GetHelpAsync(Message message);
    }
}