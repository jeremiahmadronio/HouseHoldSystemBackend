
using WebApplication2.repositories;
using WebApplication2.models;

namespace WebApplication2.service
{
    public class CommoditiyService 
    {
        private readonly ICommodityRepository _commodityRepository ;

        public CommoditiyService(ICommodityRepository commodityRepository) {
            _commodityRepository = commodityRepository ;

        }


        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _commodityRepository.GetAllCategoriesAsync();
        }







    }
}
