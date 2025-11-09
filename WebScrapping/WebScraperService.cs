using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace WebApplication2.WebScrapping
{
    public class WebScraperService : IWebScraperService
    {
        private readonly ILogger<WebScraperService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        private readonly string _baseUrl;
        private readonly string _targetUrl;
        private readonly string _saveDir;
        private readonly int _monthsLimit;

        public WebScraperService(ILogger<WebScraperService> logger, HttpClient httpClient, IConfiguration config)
        {
            _logger = logger;
            _httpClient = httpClient;
            _config = config;

            _baseUrl = _config["WebScraper:BaseUrl"] ?? "https://www.da.gov.ph";
            _targetUrl = _config["WebScraper:TargetUrl"] ?? "https://www.da.gov.ph/price-monitoring/";

            // If relative path lang sa config, convert to full path
            var folderPath = _config["WebScraper:DownloadFolder"] ?? "DownloadedPDFs";
            _saveDir = Path.IsPathRooted(folderPath)
                ? folderPath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPath);

            _monthsLimit = int.TryParse(_config["WebScraper:MonthsLimit"], out var limit) ? limit : 3;

            Directory.CreateDirectory(_saveDir); // auto-create folder if not exists
        }

        public async Task CheckAndDownloadNewPDFsAsync()
        {
            _logger.LogInformation("🔍 Checking DA website for new PDFs...");

            string html;
            try
            {
                html = await _httpClient.GetStringAsync(_targetUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to fetch HTML from DA website.");
                return;
            }

            var doc = new HtmlAgilityPack.HtmlDocument(); // ✅ avoid ambiguity
            doc.LoadHtml(html);

            var pdfLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]");
            if (pdfLinks == null || pdfLinks.Count == 0)
            {
                _logger.LogWarning("⚠️ No PDF links found.");
                return;
            }

            foreach (var link in pdfLinks)
            {
                var href = link.GetAttributeValue("href", "").Trim();
                if (string.IsNullOrEmpty(href)) continue;

                if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    href = _baseUrl + href;

                var fileName = Path.GetFileName(href);
                if (string.IsNullOrEmpty(fileName)) continue;

                // 🗓 Filter: only last N months
                if (!IsWithinLastMonths(fileName, _monthsLimit))
                {
                    _logger.LogInformation($"⏩ Skipped (older than {_monthsLimit} months): {fileName}");
                    continue;
                }

                var filePath = Path.Combine(_saveDir, fileName);

                if (!File.Exists(filePath))
                {
                    await DownloadPDFAsync(href, filePath);
                }
                else
                {
                    _logger.LogInformation($"📂 Skipped existing file: {fileName}");
                }
            }
        }

        private async Task DownloadPDFAsync(string url, string filePath)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(filePath, bytes);
                _logger.LogInformation($"✅ Downloaded: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to download PDF: {url}");
            }
        }

        // 🔍 Helper method: check kung nasa last N months base sa filename
        private bool IsWithinLastMonths(string fileName, int months)
        {
            var now = DateTime.Now;
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            DateTime fileDate;

            string[] formats = { "MMMM-yyyy", "MMM-yyyy", "MM-dd-yyyy", "MM-yyyy", "yyyy-MM-dd" };

            if (DateTime.TryParseExact(nameWithoutExt, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out fileDate))
            {
                return fileDate >= now.AddMonths(-months);
            }

            // try parse kung may month name lang (e.g., "October" or "Oct")
            if (DateTime.TryParseExact(nameWithoutExt, new[] { "MMMM", "MMM" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
            {
                fileDate = new DateTime(now.Year, fileDate.Month, 1);
                return fileDate >= now.AddMonths(-months);
            }

            _logger.LogDebug($"⚠️ Cannot parse date from filename: {fileName}");
            return false; // if no recognizable date, skip for safety
        }
    }
}
