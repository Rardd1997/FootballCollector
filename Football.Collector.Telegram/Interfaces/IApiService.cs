using Football.Collector.Common.Models;
using Football.Collector.Data.Models;
using System.Threading.Tasks;

namespace Football.Collector.Telegram.Interfaces
{
    public interface IApiService
    {
        Task<TelegramGame> CreateTelegramGameAsync(CreateTelegramGameRequest request);
        Task<TelegramGame> UpdateTelegramGameAsync(UpdateTelegramGameRequest request);
        Task<TelegramUser> FindTelegramUserAsync(FindTelegramUserRequest request);
        Task<TelegramGame> FindTelegramGameAsync(FindTelegramGameRequest request);
        Task<TelegramGame> FindLastTelegramGameAsync(FindLastTelegramGameRequest request);
        Task<TelegramGamePlayer> CreateTelegramGamePlayerAsync(CreateTelegramGamePlayerRequest request);
        Task<bool> DeleteTelegramGamePlayerAsync(DeleteTelegramGamePlayerRequest request);

        Task<TelegramUser> CreateTelegramUserAsync(CreateTelegramUserRequest request);
        Task<TelegramChatUser> FindTelegramChatUserAsync(FindTelegramChatUserRequest request);
        Task<TelegramChatUser> CreateTelegramChatUserAsync(CreateTelegramChatUserRequest request);
    }
}
