using WebApplication2.dto.DietaryTagDTO;
using WebApplication2.repositories;
using WebApplication2.models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WebApplication2.service
{
    public class ProductDietaryTagService
    {
        private readonly IProductDietaryTagRepository _productDietaryTagRepository;

        public ProductDietaryTagService(IProductDietaryTagRepository productDietaryTagRepository)
        {
            _productDietaryTagRepository = productDietaryTagRepository;
        }

        // Kukunin lahat ng commodities kasama ang tags at latest price
        public async Task<IEnumerable<DisplayProductDietaryTagDTO>> GetAllProductsWithTagsAsync()
        {
            var commodities = await _productDietaryTagRepository.GetAllCommoditiesWithOptionalTagsAsync();

            var result = commodities.Select(c =>
            {
                var latestPrice = c.Prices.OrderByDescending(p => p.DateReported).FirstOrDefault();

                return new DisplayProductDietaryTagDTO
                {
                    Id = latestPrice?.ProductPriceId ?? 0,
                    ProductName = c.ProductName,
                    Category = c.Category,
                    LatestPrice = latestPrice?.Price ?? 0,
                    DietaryTags = c.ProductDietaryTags
                                    .Select(pdt => pdt.DietaryTag.Name)
                                    .ToList()
                };
            });

            return result;
        }

        // Update tags per commodity
        public async Task UpdateCommodityTagsAsync(int commodityId, List<int> tagIds)
        {
            var commodity = await _productDietaryTagRepository.GetCommodityWithTagsAsync(commodityId);
            if (commodity == null) throw new Exception("Commodity not found");

            // Clear existing tags
            commodity.ProductDietaryTags.Clear();

            // Add new tags
            foreach (var tagId in tagIds)
            {
                commodity.ProductDietaryTags.Add(new ProductDietaryTag
                {
                    CommodityId = commodity.CommodityId,
                    DietaryTagId = tagId
                });
            }

            await _productDietaryTagRepository.SaveChangesAsync();
        }

        // Dashboard stats
        public async Task<(int totalProducts, int taggedProducts, int untaggedProducts, int totalTags)> GetDashboardStatsAsync()
        {
            var totalProducts = await _productDietaryTagRepository.GetTotalUniqueCommoditiesAsync();
            var taggedProducts = await _productDietaryTagRepository.GetTotalCommoditiesWithTagsAsync();
            var untaggedProducts = totalProducts - taggedProducts;
            var totalTags = await _productDietaryTagRepository.GetTotalUniqueTagsAsync();

            return (totalProducts, taggedProducts, untaggedProducts, totalTags);
        }

        public async Task<Commodity> GetCommodityByProductPriceIdAsync(int productPriceId)
        {
            var commodities = await _productDietaryTagRepository.GetAllCommoditiesWithOptionalTagsAsync();

            // Hanapin ang commodity kung saan nag-exist ang productPriceId
            var commodity = commodities.FirstOrDefault(c =>
                c.Prices.Any(p => p.ProductPriceId == productPriceId)
            );

            return commodity;
        }

    }
}
