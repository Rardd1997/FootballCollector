using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Football.Collector.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IConfiguration configuration;

        public Worker(
            IConfiguration configuration,
            ILogger<Worker> logger)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Football Collector Worker running at: {time}", DateTimeOffset.Now);

                stoppingToken.Register(() => logger.LogInformation($"Football Collector background task is stopping."));

                if(!double.TryParse(configuration["RecurringIntervalMin"], out var timespanMin))
                {
                    logger.LogError("RecurringIntervalMin settings not defined");
                    return;
                }

                var apiUrl = configuration["ApiServiceUrl"];
                if (string.IsNullOrEmpty(apiUrl))
                {
                    logger.LogError("ApiServiceUrl settings not defined");
                    return;
                }

                var webhookUrl = configuration["WebhookUrl"];
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    logger.LogError("WebhookUrl settings not defined");
                    return;
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    await PingAsync(new Uri(apiUrl));
                    await PingAsync(new Uri(webhookUrl));

                    await Task.Delay(TimeSpan.FromMinutes(timespanMin), stoppingToken);
                }

                logger.LogInformation($"Football Collector background task is stopping.");
            }
        }

        protected async Task PingAsync(Uri uriPathAndQuery)
        {
            try
            {
                logger.LogInformation($"Start Ping to {uriPathAndQuery}");

                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        RequestUri = uriPathAndQuery,
                        Method = HttpMethod.Get
                    };

                    await client.SendAsync(request);
                    logger.LogInformation($"Finish Ping to {uriPathAndQuery}");
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}
