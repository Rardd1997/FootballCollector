using Football.Collector.Data.Models;
using System;
using System.Threading.Tasks;

namespace Football.Collector.Api.Interfaces
{
    public interface ITelegramGameRepository: IDisposable
    {
        Task<TelegramGame> CreateAsync(TelegramGame newTelegramGame);
        Task DeleteAsync(string id);
        Task<TelegramGame> FindAsync(string id);
        Task<TelegramGame> FindAsync(string chatId, string messageId);
        Task<TelegramGame> FindLastGameAsync(string chatId, DateTime date);
        Task<TelegramGamePlayer> CreateTelegramGamePlayerAsync(TelegramGamePlayer player);
        Task DeleteTelegramGamePlayerAsync(string gameId, string userId);
        Task CopyPlayersAsync(string fromGameId, string toGameId);
    }
}
