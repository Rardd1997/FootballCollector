using Football.Collector.Api.Interfaces;
using Football.Collector.Data.Context;
using Football.Collector.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Football.Collector.Api.Repositories
{
    public class TelegramUserRepository : ITelegramUserRepository
    {
        private readonly FootballCollectorDbContext context;

        public TelegramUserRepository(FootballCollectorDbContext context)
        {
            this.context = context;
        }
        public async Task<TelegramUser> CreateAsync(TelegramUser newTelegramUser)
        {
            await context.TelegramUsers.AddAsync(newTelegramUser);
            await context.SaveChangesAsync();

            return newTelegramUser;
        }
        public async Task<TelegramUser> FindAsync(string id)
        {
            return await context.TelegramUsers.FindAsync(id);
        }
        public async Task<TelegramUser> FindByTelegramIdAsync(string telegramId)
        {
            return await context.TelegramUsers.FirstOrDefaultAsync(x => x.TelegramId == telegramId);
        }
        public async Task<TelegramChatUser> CreateTelegramChatUserAsync(TelegramChatUser newTelegramChatUser)
        {
            await context.TelegramChatUsers.AddAsync(newTelegramChatUser);
            await context.SaveChangesAsync();

            return newTelegramChatUser;
        }
        public Task<TelegramChatUser> FindChatUserAsync(string telegramChatId, string telegramUserId)
        {
            return context.TelegramChatUsers
                .Include(x => x.TelegramUser)
                .FirstOrDefaultAsync(x => x.TelegramChatId == telegramChatId && x.TelegramUserId == telegramUserId);
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
