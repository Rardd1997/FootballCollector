namespace Football.Collector.Common.Models
{
    public class CreateTelegramChatUserRequest
    {
        public string TelegramChatId { get; set; }
        public string TelegramUserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
