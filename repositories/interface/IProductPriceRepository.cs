using WebApplication2.models;

namespace WebApplication2.repositories;

public interface IProductPriceRepository
{
    Task AddAsync(ProductPrice price);
    Task SaveChangesAsync();

    Task<ProductPrice> GetByCommodityAndReportAsync(int commodityId, int reportId);
    Task UpdateAsync(ProductPrice price);
    Task<ProductPrice?> GetLatestByCommodityAsync(int commodityId);
    Task<List<ProductPrice>> GetLatestTwoByCommodityAsync(int commodityId);

    Task DeleteByCommodityIdAsync(int commodityId);


}