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
            var groupedData = await _repository.GetGroupedDataAsync(query, filter.TimeRange);

            var currentAvg = groupedData.Any() ? groupedData.Average(x => (double)x.AvgPrice) : 0;

            IQueryable<ProductPrice> prevQuery;
            var today = DateTime.UtcNow.Date;

            switch (filter.TimeRange.ToLower())
            {
                case "weekly":
                    var startPrev4Weeks = StartOfWeek(today).AddDays(-28);
                    prevQuery = query.Where(p => p.DateReported >= startPrev4Weeks && p.DateReported < StartOfWeek(today));
                    break;

                case "monthly":
                    var startPrev4Months = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-7);
                    var endPrev4Months = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-4).AddDays(-1);
                    prevQuery = query.Where(p => p.DateReported >= startPrev4Months && p.DateReported <= endPrev4Months);
                    break;

                default:
                    var startPrev7Days = today.AddDays(-13);
                    var endPrev7Days = today.AddDays(-7);
                    prevQuery = query.Where(p => p.DateReported >= startPrev7Days && p.DateReported <= endPrev7Days);
                    break;
            }

            var prevData = await _repository.GetGroupedDataAsync(prevQuery, filter.TimeRange);
            var previousAvg = prevData.Any() ? prevData.Average(x => (double)x.AvgPrice) : 0;

            double percentChange = previousAvg > 0 ? ((currentAvg - previousAvg) / previousAvg) * 100 : 0;

            var mostExpensive = await query.OrderByDescending(p => p.Price)
                                           .Select(p => new { p.Commodity.ProductName, p.Price, p.unit })
                                           .FirstOrDefaultAsync();

            var cheapest = await query.OrderBy(p => p.Price)
                                      .Select(p => new { p.Commodity.ProductName, p.Price, p.unit })
                                      .FirstOrDefaultAsync();

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









    }
}
