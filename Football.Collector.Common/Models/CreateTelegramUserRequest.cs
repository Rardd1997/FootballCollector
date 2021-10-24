namespace Football.Collector.Common.Models
{
    public class CreateTelegramUserRequest
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TelegramId { get; set; }
    }
}
