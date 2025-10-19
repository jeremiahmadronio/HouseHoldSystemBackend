using WebApplication2.repositories;
using WebApplication2.models;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Globalization;
using System.Linq;
using FuzzySharp;
using WebApplication2.dto.ProductPriceDTO;

namespace WebApplication2.service
{
    public class ProductsService
    {
        private readonly ICommodityRepository _commodityRepo;
        private readonly IProductPriceRepository _priceRepo;
        private readonly IPriceReportRepository _reportRepo;
        private readonly IMarketRepository _marketRepo;

        public ProductsService(
            ICommodityRepository commodityRepo,
            IProductPriceRepository priceRepo,
            IPriceReportRepository reportRepo,
            IMarketRepository marketRepo)
        {
            _commodityRepo = commodityRepo;
            _priceRepo = priceRepo;
            _reportRepo = reportRepo;
            _marketRepo = marketRepo;
        }

    
        private string DetectUnit(string specification)
        {
            if (string.IsNullOrWhiteSpace(specification)) return "kg";

            string lower = specification.ToLower();
            if (Regex.IsMatch(lower, @"pcs\s*/\s*kg")) return "kg";
            if (Regex.IsMatch(lower, @"\bpcs?\b")) return "pcs";
            if (Regex.IsMatch(lower, @"\bkg\b")) return "kg";
            if (Regex.IsMatch(lower, @"\b(gm|g|gram)\b")) return "g";
            if (Regex.IsMatch(lower, @"\b(liter|litre|l)\b")) return "L";
            if (Regex.IsMatch(lower, @"\bml\b")) return "ml";
            if (Regex.IsMatch(lower, @"bundle")) return "bundle";
            if (Regex.IsMatch(lower, @"bottle")) return "bottle";

            return "kg";
        }

      
        public async Task<PriceReport> ProcessReportAsync(
       string fileName,
       List<(string CommodityName, string Specification, decimal? Price, string Category)> parsedData)
        {
            var report = new PriceReport
            {
                FileName = fileName,
                ReportWeek = DateTime.Now.ToString("yyyy-MM-dd"),
                UploadedBy = "Admin"
            };
            await _reportRepo.AddAsync(report);

            foreach (var item in parsedData.Distinct())
            {
                if (item.Price == null) continue;

                // Try get existing commodity
                var commodity = await _commodityRepo.GetByNameAsync(item.CommodityName);

                if (commodity == null)
                {
                    // If not exists, create new with category from parsed PDF
                    commodity = new Commodity
                    {
                        ProductName = item.CommodityName,
                        Category = string.IsNullOrEmpty(item.Category) ? "Uncategorized" : item.Category,
                        IsActive = true
                    };
                    await _commodityRepo.AddAsync(commodity);
                }
                else
                {
                    // Update category if null or empty
                    if (string.IsNullOrEmpty(commodity.Category) && !string.IsNullOrEmpty(item.Category))
                    {
                        commodity.Category = item.Category;
                        await _commodityRepo.UpdateAsync(commodity);
                    }
                }

                string detectedUnit = DetectUnit(item.Specification);

                var existingPrice = await _priceRepo.GetByCommodityAndReportAsync(commodity.CommodityId, report.ReportId);
                if (existingPrice != null)
                {
                    existingPrice.Price = item.Price.Value;
                    existingPrice.unit = detectedUnit;
                    existingPrice.DateReported = DateTime.UtcNow;
                    await _priceRepo.UpdateAsync(existingPrice);
                }
                else
                {
                    var newPrice = new ProductPrice
                    {
                        CommodityId = commodity.CommodityId,
                        ReportId = report.ReportId,
                        Price = item.Price.Value,
                        unit = detectedUnit,
                        DateReported = DateTime.UtcNow
                    };
                    await _priceRepo.AddAsync(newPrice);
                }
            }

            return report;
        }

        public async Task<List<(string CommodityName, string Specification, decimal? Price, string Category)>> ProcessPdfWithTabula(IFormFile file)
        {
            var parsedData = new List<(string CommodityName, string Specification, decimal? Price, string Category)>();
            var tempPdfPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
            var outputJsonPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

            using (var stream = File.Create(tempPdfPath))
                await file.CopyToAsync(stream);

            string tabulaJarPath = @"C:\Users\Home\Downloads\tabula-1.0.5-jar-with-dependencies.jar";

            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "java";
            process.StartInfo.Arguments = $"-jar \"{tabulaJarPath}\" -f JSON --spreadsheet --use-line-returns -p all \"{tempPdfPath}\" -o \"{outputJsonPath}\"";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            string error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (!File.Exists(outputJsonPath))
            {
                Console.WriteLine("❌ Tabula failed to extract tables.");
                Console.WriteLine(error);
                return parsedData;
            }

            string jsonContent = await File.ReadAllTextAsync(outputJsonPath);
            var tables = System.Text.Json.JsonSerializer.Deserialize<List<TabulaTable>>(jsonContent);

            string currentCategory = "Uncategorized";

            // Mapping table for headers
            var categoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "OTHER BASIC COMMODITIES", "Other Basic Commodities" },
        { "FRUITS", "Fruits" },
        { "SPICES", "Spices" },
        { "HIGHLAND VEGETABLES", "Highland Vegetables" },
        { "LOWLAND VEGETABLES", "Lowland Vegetables" },
        { "POULTRY PRODUCTS", "Poultry Products" },
        { "OTHER LIVESTOCK MEAT PRODUCTS", "Other Livestock Meat Products" },
        { "PORK MEAT PRODUCTS", "Pork Meat Products" },
        { "FISH PRODUCTS", "Fish Products" },
        { "CORN PRODUCTS", "Corn Products" },
        { "LOCAL COMMERCIAL RICE", "Local Commercial Rice" },
        { "IMPORTED COMMERCIAL RICE", "Imported Commercial Rice" },
        { "BEEF MEAT PRODUCTS", "Beef Meat Products" }
    };

            foreach (var table in tables)
            {
                foreach (var row in table.data)
                {
                    if (row.Count < 2) continue;

                    string commodityRaw = row.ElementAtOrDefault(0)?.text ?? "";
                    string spec = row.ElementAtOrDefault(1)?.text?.Trim() ?? "";
                    string priceStr = row.ElementAtOrDefault(2)?.text?.Trim() ?? "";

                    // Normalize: trim + collapse multiple spaces
                    string commodity = Regex.Replace(commodityRaw, @"\s+", " ").Trim();

                    // Remove non-digit characters for price
                    priceStr = Regex.Replace(priceStr, @"[^\d\.]", "");

                    if (string.IsNullOrWhiteSpace(commodity) || commodity.Length < 2)
                        continue;

                    // Check if this is a header using mapping table
                    string key = commodity.ToUpper();
                    if (categoryMap.TryGetValue(key, out string mappedCategory))
                    {
                        currentCategory = mappedCategory;
                        Console.WriteLine($"📌 Detected new category: {currentCategory}");
                        continue; // skip header row
                    }

                    // Optional: normalize rice products
                    string normalizedCommodity = commodity;
                    if (Regex.IsMatch(commodity, @"(special\s*rice|premium|well\s*milled|regular\s*milled)", RegexOptions.IgnoreCase))
                    {
                        normalizedCommodity = "Rice " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(commodity.ToLower());
                        if (currentCategory.ToUpper().Contains("LOCAL"))
                            normalizedCommodity += " (Local)";
                    }

                    if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                    {
                        parsedData.Add((normalizedCommodity, spec, price, currentCategory));
                        Console.WriteLine($"✔ Parsed: {normalizedCommodity} | {spec} | {price} | Category: {currentCategory}");
                    }
                }
            }

            File.Delete(tempPdfPath);
            File.Delete(outputJsonPath);

            Console.WriteLine($"✅ Total parsed rows: {parsedData.Count}");
            return parsedData;
        }



        // Helper classes
        public class TabulaTable
        {
            public List<List<TabulaCell>> data { get; set; }
        }

        public class TabulaCell
        {
            public string text { get; set; }
        }






        public async Task<List<DisplayProductPriceDTO>> GetAllProductPriceDisplayAsync()
        {
            var commodities = await _commodityRepo.GetAllCommoditiesAsync();
            var result = new List<DisplayProductPriceDTO>();

            foreach (var commodity in commodities)
            {
                var prices = await _priceRepo.GetLatestTwoByCommodityAsync(commodity.CommodityId);

                var latestPriceEntry = prices.FirstOrDefault();
                var previousPriceEntry = prices.Count > 1 ? prices[1] : null;

                decimal latest = latestPriceEntry?.Price ?? 0;
                decimal? previous = previousPriceEntry?.Price;

                string status = "N/A";
                if (previous.HasValue)
                {
                    if (latest > previous.Value) status = "Up";
                    else if (latest < previous.Value) status = "Down";
                    else status = "Same";
                }

                result.Add(new DisplayProductPriceDTO
                {
                    id = commodity.CommodityId,
                    ProductName = commodity.ProductName,
                    Category = string.IsNullOrEmpty(commodity.Category) ? "N/A" : commodity.Category,
                    LatestPrice = latest,
                    PreviousPrice = previous,
                    Status = status,
                    LatestPriceDate = latestPriceEntry?.Report?.UploadDate // galing sa PriceReport
                });
            }

            return result;
        }

    }
}
