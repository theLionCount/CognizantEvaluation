using Common.Data;
using Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DataLoader
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IServiceScopeFactory scopeFactory;

        private readonly IHostApplicationLifetime appLifetime;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            this.scopeFactory = scopeFactory;
            this.appLifetime = appLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("downloading airports...");
            var airports = await StreamWithNewtonsoftJson("https://raw.githubusercontent.com/jbrooksuk/JSON-Airports/master/airports.json", new HttpClient());
            _logger.LogInformation("airports downloaded");

            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AirportContext>();

            _logger.LogInformation("resetting database");
            context.Database.EnsureDeleted();
            context.Database.Migrate();

            _logger.LogInformation("saving airports...");
            await context.airports.AddRangeAsync(airports.Where(t => t.continent == Continent.EU));

            await context.SaveChangesAsync();

            _logger.LogInformation("done");

            appLifetime.StopApplication();
        }

        private async Task<List<Airport>> StreamWithNewtonsoftJson(string uri, HttpClient httpClient)
        {
            using var httpResponse = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

            if (httpResponse.Content is object)
            {
                var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);

                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                try
                {
                    return serializer.Deserialize<List<Airport>>(jsonReader);
                }
                catch (JsonReaderException)
                {
                   _logger.LogInformation("Invalid JSON.");
                }
            }
            else
            {
                _logger.LogInformation("HTTP Response was invalid and cannot be deserialised.");
            }

            return null;
        }
    }
}
