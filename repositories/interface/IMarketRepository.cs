using WebApplication2.models;

namespace WebApplication2.repositories;

public interface IMarketRepository
{
	Task<Market?> GetByIdAsync(int id);
	Task AddAsync(Market market);
	Task<IEnumerable<Market>> GetAllAsync();

    
    Task UpdateAsync(Market market);
    Task DeleteAsync(int id);
    
    Task DeleteBulkAsync(IEnumerable<int> ids);
}