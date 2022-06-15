namespace Football.Collector.Common.Models
{
    public class CreateTelegramGameRequest : UpdateTelegramGameRequest
    {
        public string LastGameId { get; set; }
    }
}