using Football.Collector.Data.Context;
using Football.Collector.Data.Models;
using Football.Collector.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Football.Collector.Api.Extensions
{
    public static class FootballCollectorDbContextExtensions
    {
        public static void InitializeDatabase(this FootballCollectorDbContext context, IEncryptionService encryptionService)
        {
            if (!context.ServiceUsers.Any())
            {
                var serviceUsers = new List<ServiceUser>
                {
                    new ServiceUser
                    {
                        Username = "TelegramApp",
                        Password = encryptionService.GetPasswordHash("TelegramApp@2021")
                    }
                };

                context.AddRange(serviceUsers);
                context.SaveChanges();
            }
        }
    }
}
