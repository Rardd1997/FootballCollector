using System;

namespace Football.Collector.Common.Models
{
    public class CreateTelegramGameRequest : FindTelegramGameRequest
    {
        public DateTime Date { get; set; }
        public int DurationInMins { get; set; }
        public string Address { get; set; }
        public double Cost { get; set; }
        public string LastGameId { get; set; }
    }
}
