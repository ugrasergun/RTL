using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RTL.API.Models.Parsers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RTL.API.Services
{
    public class ScraperService : IHostedService
    {
        private Timer _timer;
        private HttpClient _client;
        private readonly IShowService _showService;
        private readonly ILogger _logger;
        private IConfiguration _configuration;
        private ShowParser _parser;

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static object _lockObject = new object();


        public ScraperService(HttpClient client, IShowService showService, ILogger<ScraperService> logger, IConfiguration configuration, ShowParser parser)
        {
            _client = client;
            _showService = showService;
            _logger = logger;
            _configuration = configuration;
            _parser = parser;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var interval = (int)_configuration.GetSection("Shows").GetValue(typeof(int), "IntervalMinutes");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(interval));

            return Task.CompletedTask;
        }

        public void DoWork(object state)
        {
            bool locked = false;
            try
            {
                locked = Monitor.TryEnter(_lockObject);

                if (locked)
                {
                    var APISection = _configuration.GetSection("Shows:API");
                    var maxRequest = (int)APISection.GetValue(typeof(int), "MaxRequestNumber");
                    SemaphoreSlim rateLimiter = new SemaphoreSlim(maxRequest);
                    var tasks = new List<Task>();

                    var updatesResponse = _client.GetAsync(APISection.GetSection("Domain").Value + APISection.GetSection("UpdatesAPI").Value).Result;
                    if (updatesResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogWarning( $"ScraperService Updates call result: {updatesResponse.ReasonPhrase}");
                        return;
                    }
                    var updateContent = updatesResponse.Content.ReadAsStringAsync().Result;
                    dynamic updates = JObject.Parse(updateContent);
                    foreach (JProperty update in updates)
                    {
                        var showId = int.Parse(update.Name);
                        var LastUpdateTime = UnixEpoch.AddSeconds((long)((JValue)update.Value).Value);
                        var localShow = _showService.GetShowByShowId(showId);
                        if (localShow?.LastUpdateTime >= LastUpdateTime)
                        {
                            _logger.LogInformation($"Show \"{localShow.ShowName}\" is already up to date");
                            continue;
                        }
                        var task = Task.Run(async () =>
                        {
                            await rateLimiter.WaitAsync();
                            try
                            {
                                var showResponse = await _client.GetAsync(string.Format(APISection.GetSection("Domain").Value + APISection.GetSection("ShowAPI").Value, showId));

                                if (updatesResponse.StatusCode != System.Net.HttpStatusCode.OK)
                                {
                                    _logger.LogError($"ShowService call result for id:\"{showId}\" = {updatesResponse.ReasonPhrase}");
                                    return;
                                }

                                var showContent = await showResponse.Content.ReadAsStringAsync();

                                var show = _parser.ParseShowFromJSON(showContent);

                                _showService.Upsert(show);
                                _logger.LogInformation($"\"{show.ShowName}\" has been inserted/updated");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Unhandled Exception: {ex.Message} \nStackTrace: {ex.StackTrace}");
                            }
                            finally
                            {
                                var intervalSeconds = (int)APISection.GetValue(typeof(int), "MaxRequestIntervalSeconds");
                                await Task.Delay(intervalSeconds * 1000);
                                rateLimiter.Release();
                            }
                        });
                        tasks.Add(task);
                    }
                    Task.WhenAll(tasks).Wait();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled Exception: {ex.Message} \nStackTrace: {ex.StackTrace}");
            }
            finally
            {
                if (locked) Monitor.Exit(_lockObject);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
