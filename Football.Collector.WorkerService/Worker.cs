using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Football.Collector.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly WorkerOptions options;

        public Worker(
            IOptions<WorkerOptions> options,
            ILogger<Worker> logger)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Football Collector Worker running at: {time}", DateTimeOffset.Now);

                stoppingToken.Register(() => logger.LogInformation($"Football Collector background task is stopping."));

                if(options.RecurringIntervalMin == 0)
                {
                    logger.LogError("RecurringIntervalMin settings not defined");
                    return;
                }

                if (string.IsNullOrEmpty(options.ApiServiceUrl))
                {
                    logger.LogError("ApiServiceUrl settings not defined");
                    return;
                }

                if (string.IsNullOrEmpty(options.WebhookUrl))
                {
                    logger.LogError("WebhookUrl settings not defined");
                    return;
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    await PingAsync(new Uri(options.ApiServiceUrl));
                    await PingAsync(new Uri(options.WebhookUrl));

                    await Task.Delay(TimeSpan.FromMinutes(options.RecurringIntervalMin), stoppingToken);
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
