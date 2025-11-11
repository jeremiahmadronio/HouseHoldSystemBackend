using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using WebApplication2.service;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace WebApplication2.WebScrapping
{
    public class WebScraperService : IWebScraperService
    {
        private readonly ILogger<WebScraperService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ProductsService _productsService;

        private readonly string _baseUrl;
        private readonly string _targetUrl;
        private readonly string _saveDir;
        private readonly int _monthsLimit;

        public WebScraperService(
            ILogger<WebScraperService> logger,
            HttpClient httpClient,
            IConfiguration config,
            ProductsService productsService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _config = config;
            _productsService = productsService;

            _baseUrl = _config["WebScraper:BaseUrl"] ?? "https://www.da.gov.ph";
            _targetUrl = _config["WebScraper:TargetUrl"] ?? "https://www.da.gov.ph/price-monitoring/";

            var folderPath = _config["WebScraper:DownloadFolder"] ??
                             Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DA_PDFs");

            _saveDir = Path.IsPathRooted(folderPath) ? folderPath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPath);

            _monthsLimit = int.TryParse(_config["WebScraper:MonthsLimit"], out var limit) ? limit : 3;

            Directory.CreateDirectory(_saveDir);
            _logger.LogInformation($" PDF save directory: {_saveDir}");
        }

        public async Task CheckAndDownloadNewPDFsAsync()
        {
            _logger.LogInformation(" Checking DA website for new PDFs...");

            string html;
            try
            {
                html = await _httpClient.GetStringAsync(_targetUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to fetch HTML from DA website.");
                return;
            }

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var pdfLinks = doc.DocumentNode.SelectNodes("//table[@id='tablepress-112']//a[contains(@href, '.pdf')]");
            if (pdfLinks == null || pdfLinks.Count == 0)
            {
                _logger.LogWarning(" No PDF links found in the table.");
                return;
            }

            foreach (var link in pdfLinks)
            {
                var href = link.GetAttributeValue("href", "").Trim();
                if (string.IsNullOrEmpty(href)) continue;

                var fileName = Path.GetFileName(href);
                if (string.IsNullOrEmpty(fileName)) continue;

                
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var match = Regex.Match(nameWithoutExt, @"([A-Za-z]+-\d{1,2}-\d{4})");
                if (!match.Success)
                {
                    _logger.LogWarning($"⚠️ Cannot extract date from filename: {fileName}");
                    continue; 
                }

                string datePart = match.Groups[1].Value;
                if (!DateTime.TryParseExact(datePart,
                    new[] { "MMMM-d-yyyy", "MMMM-dd-yyyy", "MMM-d-yyyy", "MMM-dd-yyyy" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileDate))
                {
                    _logger.LogWarning($" Cannot parse date from extracted part: {datePart}");
                    continue; 
                }

               
                if (fileDate < DateTime.Now.AddMonths(-_monthsLimit))
                {
                    _logger.LogInformation($" Skipped (older than {_monthsLimit} months): {fileName}");
                    continue;
                }

                var filePath = Path.Combine(_saveDir, fileName);

                
                if (File.Exists(filePath))
                {
                    _logger.LogInformation($" Skipped existing file: {fileName}");
                    continue;
                }

                
                if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    href = _baseUrl + href;

               
                await DownloadPDFAsync(href, filePath);

                
                await ProcessPDFAutomatically(filePath, fileName);
            }

            _logger.LogInformation(" Task completed.");
        }

        private async Task DownloadPDFAsync(string url, string filePath)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(filePath, bytes);
                _logger.LogInformation($" Downloaded: {Path.GetFileName(filePath)} to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $" Failed to download PDF: {url}");
            }
        }

        private async Task ProcessPDFAutomatically(string filePath, string fileName)
        {
            try
            {
                var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(filePath));
                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "file", fileName);

                var parsedData = await _productsService.ProcessPdfWithTabula(formFile);

                await _productsService.ProcessReportAsync(fileName, parsedData);

                _logger.LogInformation($"📤 Processed and saved to DB: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to process PDF automatically: {fileName}");
            }
        }
    }
}
    