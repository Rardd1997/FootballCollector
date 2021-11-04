namespace Football.Collector.Data.Models
{
    public class TelegramGameTeam
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TelegramGameId { get; set; }
        public string TelegramGamePlayerId { get; set; }

        public virtual TelegramGame TelegramGame { get; set; }
        public virtual TelegramGamePlayer TelegramGamePlayer { get; set; }
    }
}
