using Football.Collector.Common.Models;
using System;
using System.Collections.Generic;

namespace Football.Collector.Telegram.Extensions
{
    public static class TelegramGameRequestExtensions
    {
        public static bool InitFromArgs(this UpdateTelegramGameRequest @this, IDictionary<string, string> args)
        {
            if(@this == null)
            {
                return false;
            }

            var ret = false;

            foreach (var item in args)
            {
                switch (item.Key)
                {
                    case "address":
                        @this.Address = item.Value;
                        ret = true;
                        break;

                    case "cost":
                        if (double.TryParse(item.Value, out var cost))
                        {
                            @this.Cost = cost;
                            ret = true;
                        }
                        break;

                    case "date":
                        if (DateTime.TryParse(item.Value, out var date))
                        {
                            @this.Date = date;
                            ret = true;
                        }
                        break;

                    case "durationInMins":
                        if (int.TryParse(item.Value, out var durationInMins))
                        {
                            @this.DurationInMins = durationInMins;
                            ret = true;
                        }
                        break;

                    case "notes":
                        @this.Notes = item.Value;
                        ret = true;
                        break;
                }
            }

            return ret;
        }
    }
}
