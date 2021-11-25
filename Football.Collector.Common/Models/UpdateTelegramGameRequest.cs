using System;

namespace Football.Collector.Common.Models
{
    public class UpdateTelegramGameRequest : FindTelegramGameRequest
    {
        public DateTime Date { get; set; }
        public int DurationInMins { get; set; }
        public string Address { get; set; }
        public double Cost { get; set; }
        public string Notes { get; set; }
    }
}
