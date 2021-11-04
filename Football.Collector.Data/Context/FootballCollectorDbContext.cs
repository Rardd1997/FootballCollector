using Football.Collector.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Football.Collector.Data.Context
{
    public class FootballCollectorDbContext : DbContext
    {
        public FootballCollectorDbContext(DbContextOptions<FootballCollectorDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ServiceUser>(e =>
            {
                e.ToTable("ServiceUsers").HasKey(x => x.Id);

                e.Property(x => x.Id).HasDefaultValueSql("newid()").IsRequired();

                e.Property(x => x.Username).IsRequired();
                e.HasIndex(x => x.Username);

                e.Property(x => x.Password).IsRequired();
            });

            builder.Entity<TelegramUser>(e =>
            {
                e.ToTable("TelegramUsers").HasKey(x => x.Id);

                e.Property(x => x.Id).HasDefaultValueSql("newid()").IsRequired();

                e.Property(x => x.FirstName).IsRequired();
                e.Property(x => x.TelegramId).IsRequired();

                e.HasIndex(x => x.TelegramId);
            });

            builder.Entity<TelegramUserScore>(e =>
            {
                e.ToTable("TelegramUserScores").HasKey(x => x.Id);

                e.Property(x => x.Id).HasDefaultValueSql("newid()").IsRequired();

                e.Property(x => x.Score).IsRequired();

                e.HasOne(x => x.TelegramUser).WithOne().HasForeignKey<TelegramUserScore>(x => x.TelegramUserId);
            });

            builder.Entity<TelegramChatUser>(e =>
            {
                e.ToTable("TelegramChatUsers").HasKey(x => x.Id);

                e.Property(x => x.TelegramChatId).IsRequired();
                e.Property(x => x.TelegramUserId).IsRequired();

                e.HasOne(x => x.TelegramUser).WithMany().HasForeignKey(x => x.TelegramUserId);
            });

            builder.Entity<TelegramGame>(e =>
            {
                e.ToTable("TelegramGames").HasKey(x => x.Id);

                e.Property(x => x.Id).HasDefaultValueSql("newid()").IsRequired();

                e.Property(x => x.Address).IsRequired();
                e.Property(x => x.Date).IsRequired();

                e.Property(x => x.ChatId).IsRequired();
                e.Property(x => x.MessageId).IsRequired();

                e.HasMany(x => x.TelegramGamePlayers);
            });

            builder.Entity<TelegramGamePlayer>(e =>
            {
                e.ToTable("TelegramGamePlayers").HasKey(x => x.Id);

                e.Property(x => x.Id).HasDefaultValueSql("newid()").IsRequired();
                e.Property(x => x.CreatedAt).HasDefaultValueSql("getutcdate()").IsRequired();

                e.Property(x => x.TelegramGameId).IsRequired();
                e.Property(x => x.TelegramUserId).IsRequired();

                e.HasIndex(x => new { x.TelegramGameId, x.TelegramUserId });

                e.HasOne(x => x.TelegramGame).WithMany(x => x.TelegramGamePlayers).HasForeignKey(x => x.TelegramGameId);
                e.HasOne(x => x.TelegramUser).WithMany().HasForeignKey(x => x.TelegramUserId);
            });

            builder.Entity<TelegramGameTeam>(e =>
            {
                e.ToTable("TelegramGameTeams").HasKey(x => x.Id);

                e.Property(x => x.Id).HasDefaultValueSql("newid()").IsRequired();

                e.Property(x => x.TelegramGameId).IsRequired();
                e.Property(x => x.TelegramGamePlayerId).IsRequired();

                e.HasIndex(x => new { x.TelegramGameId, x.TelegramGamePlayerId });

                e.HasOne(x => x.TelegramGame).WithMany(x => x.TelegramGameTeams).HasForeignKey(x => x.TelegramGameId);
                e.HasOne(x => x.TelegramGamePlayer).WithMany().HasForeignKey(x => x.TelegramGamePlayerId);
            });
        }

        public DbSet<ServiceUser> ServiceUsers { get; set; }
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<TelegramUserScore> TelegramUserScores { get; set; }
        public DbSet<TelegramChatUser> TelegramChatUsers { get; set; }
        public DbSet<TelegramGame> TelegramGames { get; set; }
        public DbSet<TelegramGamePlayer> TelegramGamePlayers { get; set; }
        public DbSet<TelegramGameTeam> TelegramGameTeams { get; set; }
    }
}
