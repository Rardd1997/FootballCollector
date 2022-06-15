using Football.Collector.Common.Models;
using Football.Collector.Data.Models;

namespace Football.Collector.Api.Extensions
{
    public static class TelegramGameExtensions
    {
        public static void ApplyUpdateRequest(this TelegramGame @this, UpdateTelegramGameRequest request)
        {
            if (@this == null || request == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                @this.Address = request.Address;
            }

            if (request.Cost > 0)
            {
                @this.Cost = request.Cost;
            }

            if (request.Date.ToUniversalTime() > System.DateTime.UtcNow)
            {
                @this.Date = request.Date;
            }

            if (request.DurationInMins > 0)
            {
                @this.DurationInMins = request.DurationInMins;
            }

            @this.HasShower = request.HasShower;
            @this.HasChangingRoom = request.HasChangingRoom;
            @this.HasParking = request.HasParking;
            @this.Type = request.Type;
        }
    }
}