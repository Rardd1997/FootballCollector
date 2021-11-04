namespace Football.Collector.Data.Models
{
    public class TelegramUserScore
    {
        public string Id { get; set; }
        public double Score { get; set; }
        public string TelegramUserId { get; set; }

        public virtual TelegramUser TelegramUser { get; set; }
    }
}
