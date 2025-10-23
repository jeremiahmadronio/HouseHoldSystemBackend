using WebApplication2.models;

namespace WebApplication2.repositories;

public interface IPriceReportRepository
{
    Task<PriceReport> AddAsync(PriceReport report);
    Task SaveChangesAsync();

    Task<IEnumerable<PriceReport>> getAllAsync();

}