using Football.Collector.Api.Interfaces;
using Football.Collector.Data.Context;
using Football.Collector.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Football.Collector.Api.Repositories
{
    public class TelegramGameRepository : ITelegramGameRepository
    {
        private readonly FootballCollectorDbContext context;

        public TelegramGameRepository(FootballCollectorDbContext context)
        {
            this.context = context;
        }
        public async Task<TelegramGame> CreateAsync(TelegramGame newTelegramGame)
        {
            await context.TelegramGames.AddAsync(newTelegramGame);
            await context.SaveChangesAsync();

            return newTelegramGame;
        }
        public async Task DeleteAsync(string id)
        {
            var game = await FindAsync(id);
            if (game != null)
            {
                context.TelegramGames.Remove(game);
            }

            await context.SaveChangesAsync();
        }
        public async Task<TelegramGame> FindAsync(string id)
        {
            var game = await context.TelegramGames
                .Include(x => x.TelegramGamePlayers)
                .ThenInclude(x => x.TelegramUser)
                .FirstOrDefaultAsync(x => x.Id == id);
            return game;
        }
        public async Task<TelegramGame> FindAsync(string chatId, string messageId)
        {
            var telegramGame = await context.TelegramGames
                .Include(x => x.TelegramGamePlayers)
                .ThenInclude(x => x.TelegramUser)
                .FirstOrDefaultAsync(x => x.ChatId == chatId && x.MessageId == messageId);

            return telegramGame;
        }
        public async Task<TelegramGame> FindLastGameAsync(string chatId, DateTime date)
        {
            var lastGames = await context.TelegramGames
                .Where(x => x.ChatId == chatId)
                .Include(x => x.TelegramGamePlayers)
                .ThenInclude(x => x.TelegramUser)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var lastGame = lastGames
                .Where(x => x.Date.DayOfWeek == date.DayOfWeek)
                .FirstOrDefault();

            if (lastGame == null)
            {
                lastGame = await context.TelegramGames
                    .Where(x => x.ChatId == chatId)
                    .Include(x => x.TelegramGamePlayers)
                    .ThenInclude(x => x.TelegramUser)
                    .OrderByDescending(x => x.Date)
                    .LastOrDefaultAsync();
            }

            return lastGame;
        }
        public async Task<TelegramGamePlayer> CreateTelegramGamePlayerAsync(TelegramGamePlayer player)
        {
            await context.TelegramGamePlayers.AddAsync(player);
            await context.SaveChangesAsync();

            return player;
        }
        public async Task DeleteTelegramGamePlayerAsync(string gameId, string userId)
        {
            var game = await FindAsync(gameId);
            if (game.TelegramGamePlayers != null)
            {
                var player = game.TelegramGamePlayers.FirstOrDefault(x => x.TelegramUserId == userId);
                if (player != null)
                {
                    context.TelegramGamePlayers.Remove(player);
                }
            }

            await context.SaveChangesAsync();
        }
        public async Task CopyPlayersAsync(string fromGameId, string toGameId)
        {
            var fromGame = await FindAsync(fromGameId);
            if (fromGame.TelegramGamePlayers != null)
            {
                foreach (var item in fromGame.TelegramGamePlayers)
                {
                    var player = new TelegramGamePlayer()
                    {
                        TelegramGameId = toGameId,
                        TelegramUserId = item.TelegramUserId
                    };

                    await context.TelegramGamePlayers.AddAsync(player);
                }
            }

            await context.SaveChangesAsync();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
