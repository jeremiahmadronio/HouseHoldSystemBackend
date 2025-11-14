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
using System.Text.Json;
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

        // ✅ Detects measurement unit
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

        // 🟢 Extract date from filename only
        private DateTime ExtractReportDateFromFileName(string fileName)
        {
            // Match Month-Day-Year (hyphen or underscore)
            var fileDateMatch = Regex.Match(fileName,
                @"(January|February|March|April|May|June|July|August|September|October|November|December)[\-_](\d{1,2})[\-_](\d{4})",
                RegexOptions.IgnoreCase);

            if (fileDateMatch.Success)
            {
                string month = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fileDateMatch.Groups[1].Value.ToLower());
                string day = fileDateMatch.Groups[2].Value;
                string year = fileDateMatch.Groups[3].Value;

                string combined = $"{month} {day}, {year}";

                if (DateTime.TryParseExact(combined, "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileDate))
                {
                    Console.WriteLine($"📅 Found date in filename: {fileDate:MMMM dd, yyyy}");
                    return fileDate.ToUniversalTime();
                }
            }

            Console.WriteLine("⚠ No date found in filename. Using UTC Now.");
            return DateTime.UtcNow;
        }

        // 🟢 PDF parsing with Tabula
        public async Task<List<(string CommodityName, string Specification, decimal? Price, string Category, DateTime ReportDate)>> ProcessPdfWithTabula(IFormFile file)
        {
            var parsedData = new List<(string, string, decimal?, string, DateTime)>();
            var tempPdfPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
            var outputJsonPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

            using (var stream = File.Create(tempPdfPath))
                await file.CopyToAsync(stream);

            // ✅ Extract date from filename
            DateTime reportDate = ExtractReportDateFromFileName(file.FileName);

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
            var tables = JsonSerializer.Deserialize<List<TabulaTable>>(jsonContent);

            string currentCategory = "Uncategorized";

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

                    string commodity = Regex.Replace(commodityRaw, @"\s+", " ").Trim();
                    priceStr = Regex.Replace(priceStr, @"[^\d\.]", "");

                    if (string.IsNullOrWhiteSpace(commodity) || commodity.Length < 2)
                        continue;

                    string key = commodity.ToUpper();
                    if (categoryMap.TryGetValue(key, out string mappedCategory))
                    {
                        currentCategory = mappedCategory;
                        Console.WriteLine($"📌 Detected new category: {currentCategory}");
                        continue;
                    }

                    string normalizedCommodity = commodity;
                    if (Regex.IsMatch(commodity, @"(special\s*rice|premium|well\s*milled|regular\s*milled)", RegexOptions.IgnoreCase))
                    {
                        normalizedCommodity = "Rice " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(commodity.ToLower());
                        if (currentCategory.ToUpper().Contains("LOCAL"))
                            normalizedCommodity += " (Local)";
                    }

                    if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                    {
                        parsedData.Add((normalizedCommodity, spec, price, currentCategory, reportDate));
                        Console.WriteLine($"✔ Parsed: {normalizedCommodity} | {spec} | {price} | {currentCategory} | Date: {reportDate:MMMM dd, yyyy}");
                    }
                }
            }

            File.Delete(tempPdfPath);
            File.Delete(outputJsonPath);

            Console.WriteLine($"✅ Total parsed rows: {parsedData.Count}");
            return parsedData;
        }

        // 🟢 Save to DB
        public async Task<PriceReport> ProcessReportAsync(string fileName,
            List<(string CommodityName, string Specification, decimal? Price, string Category, DateTime ReportDate)> parsedData)
        {
            if (parsedData.Count == 0)
                throw new Exception("No data to process.");

            DateTime reportDate = parsedData.First().ReportDate;
            int weekOfMonth = (int)Math.Ceiling(reportDate.Day / 7.0);
            string reportWeek = $"{reportDate:MMMM} Week {weekOfMonth}";

            var report = new PriceReport
            {
                FileName = string.IsNullOrEmpty(fileName) ? "ADDED BY ADMIN" : fileName,
                ReportWeek = reportWeek,
                UploadDate = reportDate,
                UploadedBy = "Admin"
            };

            await _reportRepo.AddAsync(report);

            foreach (var item in parsedData)
            {
                if (item.Price == null) continue;

                var commodity = await _commodityRepo.GetByNameAsync(item.CommodityName);
                if (commodity == null)
                {
                    commodity = new Commodity
                    {
                        ProductName = item.CommodityName,
                        Category = string.IsNullOrEmpty(item.Category) ? "Uncategorized" : item.Category,
                        IsActive = true
                    };
                    await _commodityRepo.AddAsync(commodity);
                }

                string detectedUnit = DetectUnit(item.Specification);

                var newPrice = new ProductPrice
                {
                    CommodityId = commodity.CommodityId,
                    ReportId = report.ReportId,
                    Price = item.Price.Value,
                    unit = detectedUnit,
                    DateReported = item.ReportDate
                };

                await _priceRepo.AddAsync(newPrice);
            }

            return report;
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
                // KUHAIN lahat ng price entries (hindi lang dalawa)
                var prices = await _priceRepo.GetLatestTwoByCommodityAsync(commodity.CommodityId);

                // ORDER by NEWEST → OLDEST
                prices = prices
                    .Where(p => p.DateReported != null)
                    .OrderByDescending(p => p.DateReported)
                    .ToList();

                // NO VALID PRICE AT ALL
                var latestPrice = prices.FirstOrDefault(p => p.Price > 0);
                if (latestPrice == null)
                {
                    result.Add(new DisplayProductPriceDTO
                    {
                        id = commodity.CommodityId,
                        ProductName = commodity.ProductName,
                        Category = string.IsNullOrEmpty(commodity.Category) ? "N/A" : commodity.Category,
                        Unit = "N/A",
                        LatestPrice = 0,
                        PreviousPrice = null,
                        Status = "N/A",
                        LatestPriceDate = null
                    });
                    continue;
                }

                // FIND TRUE PREVIOUS (first old price that is > 0)
                var previousPrice = prices
                    .Where(p => p.Price > 0 && p.DateReported < latestPrice.DateReported)
                    .OrderByDescending(p => p.DateReported)
                    .FirstOrDefault();

                decimal latest = latestPrice.Price;
                decimal? previous = previousPrice?.Price;

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
                    Unit = latestPrice.unit ?? "N/A",
                    LatestPrice = latest,
                    PreviousPrice = previous,
                    Status = status,
                    LatestPriceDate = latestPrice.DateReported
                });
            }

            return result;
        }







        public async Task AddNewProductPriceAsync(CreateProductDTO dto)
        {
            var commodity = await _commodityRepo.GetByNameAsync(dto.ProductName);

            if (commodity == null)
            {
                commodity = new Commodity
                {
                    ProductName = dto.ProductName,
                    Category = dto.Category
                };

                await _commodityRepo.AddAsync(commodity);
            }

            var today = DateTime.UtcNow;
            int dayOfMonth = today.Day;

            int weekOfMonth = (int)Math.Ceiling(dayOfMonth / 7.0);
            string reportWeek = $"{today:MMMM} Week {weekOfMonth}";

            var newReport = new PriceReport
            {
                FileName = "ADDED BY ADMIN",
                ReportWeek = reportWeek,
                UploadDate = today,
                UploadedBy = "Admin"
            };

            await _reportRepo.AddAsync(newReport);

            var newPrice = new ProductPrice
            {
                CommodityId = commodity.CommodityId,
                Price = dto.Price,
                unit = dto.Unit,
                DateReported = today,
                ReportId = newReport.ReportId
            };

            await _priceRepo.AddAsync(newPrice);
        }




        public async Task<bool> EditProductPriceByIdAsync(int commodityId, EditProductDTO dto)
        {
            // 1️⃣ Check if commodity exists
            var commodity = await _commodityRepo.GetByIdAsync(commodityId);
            if (commodity == null)
                throw new Exception("Product not found.");

            // 2️⃣ Update product name or category if changed
            bool isUpdated = false;
            if (!string.Equals(commodity.ProductName, dto.ProductName, StringComparison.OrdinalIgnoreCase))
            {
                commodity.ProductName = dto.ProductName;
                isUpdated = true;
            }

            if (!string.Equals(commodity.Category, dto.Category, StringComparison.OrdinalIgnoreCase))
            {
                commodity.Category = dto.Category;
                isUpdated = true;
            }

            if (isUpdated)
                await _commodityRepo.UpdateAsync(commodity);

            var latestPrice = await _priceRepo.GetLatestByCommodityAsync(commodity.CommodityId);
            if (latestPrice == null)
                throw new Exception("No existing price record for this product.");

            var today = DateTime.UtcNow;
            int weekOfMonth = (int)Math.Ceiling(today.Day / 7.0);
            string reportWeek = $"{today:MMMM} Week {weekOfMonth}";

            var newReport = new PriceReport
            {
                FileName = "UPDATED BY ADMIN",
                ReportWeek = reportWeek,
                UploadDate = today,
                UploadedBy = "Admin"
            };
            await _reportRepo.AddAsync(newReport);

            var updatedPrice = new ProductPrice
            {
                CommodityId = commodity.CommodityId,
                Price = dto.LatestPrice,
                unit = dto.Unit ?? latestPrice.unit,
                DateReported = today,
                ReportId = newReport.ReportId
            };

            await _priceRepo.AddAsync(updatedPrice);

            return true;
        }




        public async Task<bool> DeleteProductByIdAsync(int commodityId)
        {
            var commodity = await _commodityRepo.GetByIdAsync(commodityId);
            if (commodity == null)
                throw new Exception("Product not found.");

            await _priceRepo.DeleteByCommodityIdAsync(commodityId);

            await _commodityRepo.DeleteAsync(commodity);

            return true;
        }









    }
}
