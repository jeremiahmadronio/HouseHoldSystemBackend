using WebApplication2.models;

namespace WebApplication2.repositories;
public interface ICommodityRepository
{
    Task<Commodity?> GetByNameAsync(string commodityName);
    Task<Commodity> AddAsync(Commodity commodity);
    Task SaveChangesAsync();
    Task UpdateAsync(Commodity commodity);
    Task<List<Commodity>> GetAllCommoditiesAsync();

}