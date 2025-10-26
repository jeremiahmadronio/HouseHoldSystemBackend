using WebApplication2.models;

namespace WebApplication2.repositories;

public interface IProductDietaryTagRepository
{

    Task<IEnumerable<ProductPrice>> GetAllProductsWithOptionalTagsAsync();

    Task<IEnumerable<ProductPrice>> GetAllProductPricesByCommodityAsync(int commodityId);


    Task<ProductPrice> GetProductPriceWithTagsAsync(int productPriceId);
    Task SaveChangesAsync();

    Task<int> GetTotalUniqueCommoditiesAsync();
    Task<int> GetTotalCommoditiesWithTagsAsync();
    Task<int> GetTotalCommoditiesWithoutTagsAsync();
    Task<int> GetTotalUniqueTagsAsync();
}