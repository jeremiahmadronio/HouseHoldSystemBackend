using WebApplication2.repositories;
using WebApplication2.models;
using WebApplication2.dto.PriceAnalyticsDTO;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.service
{
    public class PriceAnalyticsService
    {
        private readonly IPriceAnalyticsRepository _repository;


        public PriceAnalyticsService(IPriceAnalyticsRepository repository)
        {
            _repository = repository;
            
        }

        public async Task<object> GetAnalyticsAsync(PriceFilterRequest filter)
        {
            var query = _repository.GetFilteredQuery(filter);

            // === Chart Data ===
            var groupedData = await _repository.GetGroupedDataAsync(query, filter.TimeRange);

            // === Current Average ===
            double currentAvg = 0;
            if (groupedData.Any())
            {
                var latestPeriod = groupedData.Max(g => g.Period);
                currentAvg = groupedData
                    .Where(g => g.Period == latestPeriod)
                    .Average(g => (double)g.AvgPrice);
            }

            // === Previous Average Query ===
            IQueryable<ProductPrice> prevQuery;
            var today = DateTime.UtcNow.Date;

            switch (filter.TimeRange.ToLower())
            {
                case "monthly":
                    var firstDayThisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var firstDayPrevMonth = firstDayThisMonth.AddMonths(-1);
                    var lastDayPrevMonth = firstDayThisMonth.AddDays(-1);

                    prevQuery = query.Where(p => p.DateReported >= firstDayPrevMonth && p.DateReported <= lastDayPrevMonth);
                    break;

                case "weekly":
                    var startPrev4Weeks = StartOfWeek(today).AddDays(-28);
                    prevQuery = query.Where(p => p.DateReported >= startPrev4Weeks && p.DateReported < StartOfWeek(today));
                    break;

                default: // daily
                    var startPrev7Days = today.AddDays(-6).Date;
                    var endPrev7Days = today.Date;
                    prevQuery = query.Where(p => p.DateReported.Date >= startPrev7Days && p.DateReported.Date <= endPrev7Days);
                    break;
            }

            // === Previous Average ===
            var prevData = await _repository.GetGroupedDataAsync(prevQuery, filter.TimeRange);
            double previousAvg = prevData.Any() ? prevData.Average(x => (double)x.AvgPrice) : 0;

            double percentChange = previousAvg > 0
                ? ((currentAvg - previousAvg) / previousAvg) * 100
                : 0;

            // === Most Expensive / Cheapest ===
            var mostExpensive = await query.OrderByDescending(p => p.Price)
                .Select(p => new { p.Commodity.ProductName, p.Price, p.unit })
                .FirstOrDefaultAsync();

            var cheapest = await query.OrderBy(p => p.Price)
                .Select(p => new { p.Commodity.ProductName, p.Price, p.unit })
                .FirstOrDefaultAsync();

            // ===================================================================  
            //   FIXED PRICE CHANGE LOGIC  
            //   Totoong latest & previous, no zero fallback  
            // ===================================================================

            var priceChanges = await query
                .GroupBy(p => p.Commodity.ProductName)
                .Select(g => new
                {
                    Product = g.Key,
                    Category = g.First().Commodity.Category,

                    Prices = g.Where(x => x.Price > 0)
                              .OrderByDescending(x => x.DateReported)
                              .Select(x => new { x.Price, x.DateReported })
                              .ToList()
                })
                .ToListAsync();

            var withChange = priceChanges.Select(item =>
            {
                double? current = item.Prices.Count >= 1 ? (double?)item.Prices[0].Price : null;
                double? previous = item.Prices.Count >= 2 ? (double?)item.Prices[1].Price : null;

                double percent = (previous.HasValue && previous.Value > 0)
                    ? ((current.Value - previous.Value) / previous.Value) * 100
                    : 0;

                return new
                {
                    item.Product,
                    item.Category,
                    Current = current,
                    Previous = previous,
                    PercentChange = percent
                };
            })
            .ToList();

            var topIncrease = withChange.OrderByDescending(x => x.PercentChange).Take(5);
            var topDrop = withChange.OrderBy(x => x.PercentChange).Take(5);

            return new
            {
                CurrentAveragePrice = currentAvg,
                PreviousAveragePrice = previousAvg,
                PercentChange = percentChange,
                MostExpensiveProduct = mostExpensive,
                CheapestProduct = cheapest,
                TopPriceIncreases = topIncrease,
                TopPriceDrops = topDrop,
                ChartData = groupedData
            };
        }



        private DateTime StartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }



        public async Task<List<dynamic>> GetWeeklyChartDataAsync(PriceFilterRequest filter, int lastWeeks = 4)
        {
            // Kunin ang filtered query base sa category o filters
            var query = _repository.GetFilteredQuery(filter);

            // Tawagin ang repository method para sa weekly chart data
            var weeklyData = await _repository.GetWeeklyChartDataAsync(query, lastWeeks);

            // Transform para sa frontend
            var transformed = weeklyData.Select(x => new
            {
                week = x.WeekStart.ToString("yyyy-MM-dd"), // ISO string
                category = x.Category,
                avgPrice = x.AvgPrice
            }).ToList<dynamic>();

            return transformed;
        }






    }
}
