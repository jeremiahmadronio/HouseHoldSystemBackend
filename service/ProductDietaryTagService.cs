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


        public async Task<IEnumerable<DisplayProductDietaryTagDTO>> GetAllProductsWithTagsAsync()
        {
            var data = await _productDietaryTagRepository.GetAllProductsWithOptionalTagsAsync();

            var result = data.Select(pp => new DisplayProductDietaryTagDTO
            {
                Id = pp.ProductPriceId,
                ProductName = pp.Commodity.ProductName,
                Category = pp.Commodity.Category,
                LatestPrice = pp.Price,
                DietaryTags = pp.ProductDietaryTags
                                .Select(pdt => pdt.DietaryTag.Name)
                                .ToList() 
            });

            return result;
        }




        public async Task UpdateProductTagsAsync(int productPriceId, List<int> tagIds)
        {
            var productPrice = await _productDietaryTagRepository.GetProductPriceWithTagsAsync(productPriceId);
            if (productPrice == null) throw new Exception("Product not found");

            var commodityId = productPrice.CommodityId;

            // Kunin lahat ng ProductPrice para sa commodity na iyon
            var allPricesForCommodity = await _productDietaryTagRepository.GetAllProductPricesByCommodityAsync(commodityId);

            foreach (var pp in allPricesForCommodity)
            {
                pp.ProductDietaryTags.Clear();

                foreach (var tagId in tagIds)
                {
                    pp.ProductDietaryTags.Add(new ProductDietaryTag
                    {
                        ProductPriceId = pp.ProductPriceId,
                        DietaryTagId = tagId
                    });
                }
            }

            await _productDietaryTagRepository.SaveChangesAsync();
        }


        public async Task<(int totalProducts, int taggedProducts, int untaggedProducts, int totalTags)> GetDashboardStatsAsync()
        {
            
            var totalProducts = await _productDietaryTagRepository.GetTotalUniqueCommoditiesAsync();

           
            var taggedProducts = await _productDietaryTagRepository.GetTotalCommoditiesWithTagsAsync();

           
            var untaggedProducts = totalProducts - taggedProducts;

           
            var totalTags = await _productDietaryTagRepository.GetTotalUniqueTagsAsync();

            return (totalProducts, taggedProducts, untaggedProducts, totalTags);
        }




    }


}

