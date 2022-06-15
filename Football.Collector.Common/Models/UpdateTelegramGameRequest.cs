using Football.Collector.Data.Enums;
using System;

namespace Football.Collector.Common.Models
{
    public class UpdateTelegramGameRequest : FindTelegramGameRequest
    {
        public DateTime Date { get; set; }
        public int DurationInMins { get; set; }
        public string Address { get; set; }
        public double Cost { get; set; }
        public bool HasShower { get; set; }
        public bool HasChangingRoom { get; set; }
        public bool HasParking { get; set; }
        public TelegramGameType Type { get; set; }
    }
}