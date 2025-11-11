using WebApplication2.models;
using WebApplication2.dto.PriceAnalyticsDTO;

namespace WebApplication2.repositories
{
    public interface IPriceAnalyticsRepository
    {
      

        IQueryable<ProductPrice> GetFilteredQuery(PriceFilterRequest filter);
        Task<List<dynamic>> GetGroupedDataAsync(IQueryable<ProductPrice> query, string timeRange);
        Task<List<dynamic>> GetWeeklyChartDataAsync(IQueryable<ProductPrice> query, int lastWeeks = 4);

    }
}
