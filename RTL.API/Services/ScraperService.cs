using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RTL.API.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RTL.API.Services
{
    public class ScraperService : IHostedService
    {
        private Timer _timer;
        private HttpClient _client;
        private readonly ShowService _showService;

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static object _lockObject = new object();


        public ScraperService(HttpClient client, ShowService showService)
        {
            _client = client;
            _showService = showService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            bool locked = false;
            try
            {
                locked = Monitor.TryEnter(_lockObject);

                SemaphoreSlim rateLimiter = new SemaphoreSlim(20);

                if (locked)
                {
                    var updatesResponse = _client.GetAsync("https://api.tvmaze.com/updates/shows").Result.Content.ReadAsStringAsync().Result;
                    dynamic updates = JObject.Parse(updatesResponse);
                    foreach (JProperty update in updates)
                    {
                        var showId = int.Parse(update.Name);
                        var LastUpdateTime = UnixEpoch.AddSeconds((long)((JValue)update.Value).Value);

                        if (_showService.GetShowByShowId(showId)?.LastUpdateTime == LastUpdateTime)
                            continue;

                        Task.Run(async () =>
                        {
                            await rateLimiter.WaitAsync();
                            try
                            {
                                var showResponse = _client.GetAsync($"https://api.tvmaze.com/shows/{showId}?embed=cast").Result.Content.ReadAsStringAsync().Result;

                                JObject showJson = JObject.Parse(showResponse);
                                var show = showJson.ToObject<Show>();
                                var cast = showJson.Value<JObject>("_embedded")?.Value<JArray>("cast").Select(c => c.SelectToken("person").ToObject<JObject>().ToObject<Actor>()).ToList();
                                show.Cast = cast;

                                _showService.Upsert(show);
                            }
                            finally
                            {
                                await Task.Delay(20000);
                                rateLimiter.Release();
                            }
                        });
                    }
                    
                }
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
