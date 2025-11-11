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

            // Kunin ang chart data
            var groupedData = await _repository.GetGroupedDataAsync(query, filter.TimeRange);

            // Current average (latest month / latest period)
            double currentAvg = 0;
            if (groupedData.Any())
            {
                var latestPeriod = groupedData.Max(g => g.Period);
                currentAvg = groupedData
                    .Where(g => g.Period == latestPeriod)
                    .Average(g => (double)g.AvgPrice);
            }

            IQueryable<ProductPrice> prevQuery;
            var today = DateTime.UtcNow.Date;

            switch (filter.TimeRange.ToLower())
            {
                case "monthly":
                    // Previous month
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
                    var startPrev7Days = today.AddDays(-6).Date; // last 6 days + today = 7 days
                    var endPrev7Days = today.Date;               // kasama ang today
                    prevQuery = query.Where(p => p.DateReported.Date >= startPrev7Days && p.DateReported.Date <= endPrev7Days);


                    break;
            }

            // Previous average
            var prevData = await _repository.GetGroupedDataAsync(prevQuery, filter.TimeRange);
            double previousAvg = prevData.Any() ? prevData.Average(x => (double)x.AvgPrice) : 0;

            double percentChange = previousAvg > 0 ? ((currentAvg - previousAvg) / previousAvg) * 100 : 0;

            // Most expensive / cheapest product
            var mostExpensive = await query.OrderByDescending(p => p.Price)
                                           .Select(p => new { p.Commodity.ProductName, p.Price, p.unit })
                                           .FirstOrDefaultAsync();

            var cheapest = await query.OrderBy(p => p.Price)
                                      .Select(p => new { p.Commodity.ProductName, p.Price, p.unit })
                                      .FirstOrDefaultAsync();

            // Price changes per product
            var priceChanges = await query
                .GroupBy(p => p.Commodity.ProductName)
                .Select(g => new
                {
                    Product = g.Key,
                    Category = g.First().Commodity.Category,
                    Previous = g.OrderBy(x => x.DateReported).Select(x => (double)x.Price).FirstOrDefault(),
                    Current = g.OrderByDescending(x => x.DateReported).Select(x => (double)x.Price).FirstOrDefault()
                })
                .ToListAsync();

            var withChange = priceChanges.Select(x => new
            {
                x.Product,
                x.Category,
                x.Previous,
                x.Current,
                PercentChange = x.Previous > 0 ? ((x.Current - x.Previous) / x.Previous) * 100 : 0
            }).ToList();

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
