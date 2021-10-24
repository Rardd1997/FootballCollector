using Football.Collector.Data.Models;
using System.Threading.Tasks;

namespace Football.Collector.Api.Interfaces
{
    public interface ITelegramUserRepository
    {
        Task<TelegramUser> CreateAsync(TelegramUser newTelegramUser);
        Task<TelegramUser> FindAsync(string id);
        Task<TelegramUser> FindByTelegramIdAsync(string telegramId);
        Task<TelegramChatUser> CreateTelegramChatUserAsync(TelegramChatUser newTelegramChatUser);
        Task<TelegramChatUser> FindChatUserAsync(string telegramChatId, string telegramUserId);
    }
}
