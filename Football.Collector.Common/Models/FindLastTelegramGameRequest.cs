﻿using System;

namespace Football.Collector.Common.Models
{
    public class FindLastTelegramGameRequest
    {
        public DateTime Date { get; set; }
        public string TelegramChatId { get; set; }
    }
}
