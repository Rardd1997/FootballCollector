using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Football.Collector.Telegram.Interfaces
{
    public interface IGameService
    {
        Task<Message> CreateGameAsync(Message message);
        Task<Message> CreateGamePlayerAsync(Message message);
        Task<Message> DeleteGamePlayerAsync(Message message);
        Task<Message> AddNewChatUserAsync(Chat chat, User user);
    }
}
