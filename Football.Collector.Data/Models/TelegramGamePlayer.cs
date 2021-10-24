using System;

namespace Football.Collector.Data.Models
{
    public class TelegramGamePlayer
    {
        public string Id { get; set; }
        public string TelegramGameId { get; set; }
        public string TelegramUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual TelegramGame TelegramGame { get; set; }
        public virtual TelegramUser TelegramUser { get; set; }
    }
}
