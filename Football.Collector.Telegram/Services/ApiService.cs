using Football.Collector.Common.Models;
using Football.Collector.Data.Models;
using Football.Collector.Telegram.Handlers;
using Football.Collector.Telegram.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Football.Collector.Telegram.Services
{
    public class ApiService : IApiService
    {
        public static string AccessToken { get; private set; }

        private readonly ILogger<ApiService> logger;

        public ApiService(ILogger<ApiService> logger)
        {
            this.logger = logger;
        }
        
        public async Task<TelegramGamePlayer> CreateTelegramGamePlayerAsync(CreateTelegramGamePlayerRequest request)
        {
            try
            {
                var result = await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.TelegramGamePlayerPath), HttpMethod.Post, CreateHttpContent(request));
                return JsonConvert.DeserializeObject<TelegramGamePlayer>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return null;
            }
        }
        public async Task<bool> DeleteTelegramGamePlayerAsync(DeleteTelegramGamePlayerRequest request)
        {
            try
            {
                await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.TelegramGamePlayerPath), HttpMethod.Delete, CreateHttpContent(request));
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return false;
            }
        }
        public async Task<TelegramUser> FindTelegramUserAsync(FindTelegramUserRequest request)
        {
            try
            {
                var result = await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.FindTelegramUserPath), HttpMethod.Post, CreateHttpContent(request));
                return JsonConvert.DeserializeObject<TelegramUser>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return null;
            }
        }
        public async Task<TelegramGame> FindTelegramGameAsync(FindTelegramGameRequest request)
        {
            try
            {
                var result = await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.FindTelegramGamePath), HttpMethod.Post, CreateHttpContent(request));
                return JsonConvert.DeserializeObject<TelegramGame>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return null;
            }
        }
        public async Task<TelegramGame> CreateTelegramGameAsync(CreateTelegramGameRequest request)
        {
            try
            {
                var result = await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.TelegramGamePath), HttpMethod.Post, CreateHttpContent(request));
                return JsonConvert.DeserializeObject<TelegramGame>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return null;
            }
        }
        public async Task<TelegramGame> FindLastTelegramGameAsync(FindLastTelegramGameRequest request)
        {
            try
            {
                var result = await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.FindLastTelegramGamePath), HttpMethod.Post, CreateHttpContent(request));
                return JsonConvert.DeserializeObject<TelegramGame>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return null;
            }
        }
        public async Task<TelegramUser> CreateTelegramUserAsync(CreateTelegramUserRequest request)
        {
            try
            {
                var result = await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.CreateTelegramUserPath), HttpMethod.Post, CreateHttpContent(request));
                return JsonConvert.DeserializeObject<TelegramUser>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return null;
            }
        }
        public async Task<TelegramChatUser> FindTelegramChatUserAsync(FindTelegramChatUserRequest request)
        {
            try
            {
                var result = await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.FindTelegramChatUserPath), HttpMethod.Post, CreateHttpContent(request));
                return JsonConvert.DeserializeObject<TelegramChatUser>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return null;
            }
        }
        public async Task<TelegramChatUser> CreateTelegramChatUserAsync(CreateTelegramChatUserRequest request)
        {
            try
            {
                var result = await InvokeApiAsync(new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.TelegramChatUserPath), HttpMethod.Post, CreateHttpContent(request));
                return JsonConvert.DeserializeObject<TelegramChatUser>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex);
                return null;
            }
        }


        public static async Task<string> InvokeApiAsync(Uri uriPathAndQuery, HttpMethod method, HttpContent content = null)
        {
            using (var client = new HttpClient(new AuthenticationDelegatingHandler()))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.BearerSchemeType, AccessToken);

                var request = new HttpRequestMessage
                {
                    RequestUri = uriPathAndQuery,
                    Method = method
                };

                if (content != null)
                {
                    request.Content = content;
                }

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
        public static async Task RefreshTokenAsync()
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var loggerFactory = (ILoggerFactory)serviceScope.ServiceProvider.GetService(typeof(ILoggerFactory));
                var logger = loggerFactory.CreateLogger<ApiService>();

                try
                {
                    var result = await InvokeApiAsync(
                        new Uri(new Uri(Startup.BotConfig.AppServiceApi), Constants.LoginPath),
                        HttpMethod.Post,
                        CreateHttpContent(new LoginRequest { Username = Startup.BotConfig.AppServiceLogin, Password = Startup.BotConfig.AppServicePassword }));

                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);
                    AccessToken = loginResponse.AccessToken;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString(), ex);
                }
            }
            
        }
        private static HttpContent CreateHttpContent<T>(T body)
        {
            var serializationSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore
            };
            var jsonContent = JsonConvert.SerializeObject(body, serializationSettings);

            return new StringContent(jsonContent, Encoding.UTF8, Constants.ApplicationJsonContentType);
        }
    }
}
