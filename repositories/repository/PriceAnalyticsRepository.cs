using Microsoft.EntityFrameworkCore;
using WebApplication2.data;
using WebApplication2.models;
using WebApplication2.dto.PriceAnalyticsDTO;

namespace WebApplication2.repositories.repository
{
    public class PriceAnalyticsRepository : IPriceAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public PriceAnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<ProductPrice> GetFilteredQuery(PriceFilterRequest filter)
        {
            var query = _context.ProductPrices
                .Include(p => p.Commodity)
                .AsQueryable();

            if (!string.Equals(filter.Category, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Commodity.Category == filter.Category);
            }

            return query;
        }

        public async Task<List<dynamic>> GetGroupedDataAsync(IQueryable<ProductPrice> query, string timeRange)
        {
            var data = await query.Include(p => p.Commodity).ToListAsync();
            var today = DateTime.UtcNow.Date;

            IEnumerable<dynamic> result;

            switch (timeRange.ToLower())
            {
                case "weekly":
                    var startOfLast4Weeks = StartOfWeek(today).AddDays(-21);
                    result = data
                        .Where(p => p.DateReported >= startOfLast4Weeks)
                        .GroupBy(p => new
                        {
                            Period = StartOfWeek(p.DateReported),
                            Category = p.Commodity.Category
                        })
                        .Select(g => new
                        {
                            Period = DateTime.SpecifyKind(g.Key.Period, DateTimeKind.Utc),
                            Category = g.Key.Category,
                            AvgPrice = g.Average(x => (double)x.Price)
                        })
                        .OrderBy(g => g.Period);
                    break;

                case "monthly":
                    // Group by Year & Month, specify UTC
                    var groupedByMonth = data
                        .GroupBy(p => new { p.DateReported.Year, p.DateReported.Month, Category = p.Commodity.Category })
                        .Select(g => new
                        {
                            Period = DateTime.SpecifyKind(new DateTime(g.Key.Year, g.Key.Month, 1), DateTimeKind.Utc),
                            Category = g.Key.Category,
                            AvgPrice = g.Average(x => (double)x.Price)
                        })
                        .OrderBy(g => g.Period)
                        .ToList();

                    if (!groupedByMonth.Any())
                    {
                        result = new List<dynamic>(); // Walang data
                    }
                    else
                    {
                        // Kunin latest month
                        var latestMonth = groupedByMonth.Max(g => g.Period);

                        // Kunin last 4 months data kung meron
                        var last4Months = groupedByMonth
                            .Where(g => g.Period >= latestMonth.AddMonths(-3))
                            .ToList();

                        result = last4Months.Any() ? last4Months : groupedByMonth.Where(g => g.Period == latestMonth).ToList();
                    }
                    break;

                default: // daily
                    var startOfLast7Days = today.AddDays(-6);
                    result = data
                        .Where(p => p.DateReported >= startOfLast7Days)
                        .GroupBy(p => new
                        {
                            Period = p.DateReported.Date,
                            Category = p.Commodity.Category
                        })
                        .Select(g => new
                        {
                            Period = DateTime.SpecifyKind(g.Key.Period, DateTimeKind.Utc),
                            Category = g.Key.Category,
                            AvgPrice = g.Average(x => (double)x.Price)
                        })
                        .OrderBy(g => g.Period);
                    break;
            }

            return result.ToList();
        }

        private DateTime StartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
