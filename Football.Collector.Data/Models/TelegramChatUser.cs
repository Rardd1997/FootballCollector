namespace Football.Collector.Data.Models
{
    public class TelegramChatUser
    {
        public string Id { get; set; }
        public bool IsAdmin { get; set; }
        public string TelegramChatId { get; set; }
        public string TelegramUserId { get; set; }

        public virtual TelegramUser TelegramUser { get; set; }
    }
}
