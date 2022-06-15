using System;

namespace Football.Collector.Telegram.Extensions
{
    public static class DateTimeExtensions
    {
        public static string GetDayofWeekUA(this DateTime @this)
        {
            int day = (@this.DayOfWeek == 0) ? 7 : (int)@this.DayOfWeek;

            return day switch
            {
                1 => "Понеділок",
                2 => "Вівторок",
                3 => "Середа",
                4 => "Четвер",
                5 => "П'ятниця",
                6 => "Субота",
                7 => "Неділя",
                _ => throw new NotImplementedException()
            };
        }
    }
}
