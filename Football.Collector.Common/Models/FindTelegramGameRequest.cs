namespace Football.Collector.Common.Models
{
    public class FindTelegramGameRequest
    {
        public string GameId { get; set; }
        public string ChatId { get; set; }
        public string MessageId { get; set; }
    }
}
