using WebApplication2.models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication2.repositories
{
    public interface IProductDietaryTagRepository
    {
        Task<IEnumerable<Commodity>> GetAllCommoditiesWithOptionalTagsAsync();
        Task<Commodity> GetCommodityWithTagsAsync(int commodityId);
        Task SaveChangesAsync();
        Task<int> GetTotalUniqueCommoditiesAsync();
        Task<int> GetTotalCommoditiesWithTagsAsync();
        Task<int> GetTotalCommoditiesWithoutTagsAsync();
        Task<int> GetTotalUniqueTagsAsync();


    }
}
