using Football.Collector.Data.Enums;
using System;
using System.Collections.Generic;

namespace Football.Collector.Data.Models
{
    public class TelegramGame
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public int DurationInMins { get; set; }
        public string Address { get; set; }
        public double Cost { get; set; }
        public bool HasShower { get; set; }
        public bool HasChangingRoom { get; set; }
        public bool HasParking { get; set; }
        public TelegramGameType Type { get; set; }
        public string ChatId { get; set; }
        public string MessageId { get; set; }

        public virtual ICollection<TelegramGamePlayer> TelegramGamePlayers { get; set; }
    }
}