using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication2.WebScrapping
{
    public class DailyScraperService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyScraperService> _logger;

        public DailyScraperService(IServiceProvider serviceProvider, ILogger<DailyScraperService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("✅ Background task started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scraper = scope.ServiceProvider.GetRequiredService<IWebScraperService>();
                        await scraper.CheckAndDownloadNewPDFsAsync();
                    }

                    _logger.LogInformation("✅ Task completed. Waiting 24 hours...");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in background task");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
