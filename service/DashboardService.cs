using WebApplication2.repositories;
using WebApplication2.dto.ProductPriceDTO;

namespace WebApplication2.service
{
    public class DashboardService
    {
        private readonly IUserRepository _userRepo;
        private readonly ICommodityRepository _commodityRepo;
        private readonly IProductDietaryTagRepository _productDietaryTagRepo;
        private readonly IDietaryTagRepository _dietaryTagRepo;
        private readonly IProductPriceRepository _productPriceRepo;

        public DashboardService(
            IUserRepository userRepo,
            ICommodityRepository commodityRepo,
            IProductDietaryTagRepository productDietaryTagRepo,
            IDietaryTagRepository dietaryTagRepo,
            IProductPriceRepository productPriceRepository)
        {
            _userRepo = userRepo;
            _commodityRepo = commodityRepo;
            _productDietaryTagRepo = productDietaryTagRepo;
            _dietaryTagRepo = dietaryTagRepo;
            _productPriceRepo = productPriceRepository;
        }

        public async Task<DashboadAdminDTO> GetDashboardSummaryAsync()
        {
            var totalUsers = _userRepo.GetTotalUser() ?? 0;

            var allCommodities = await _commodityRepo.GetAllCommoditiesAsync();
            var totalProducts = allCommodities?.Count() ?? 0;

            var totalProductsWithTags = await _productDietaryTagRepo.GetTotalCommoditiesWithTagsAsync();

            var allTags = await _dietaryTagRepo.getAllAsync();
            var totalDietaryTags = allTags?.Count() ?? 0;

            return new DashboadAdminDTO
            {
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalProductsWithTags = totalProductsWithTags,
                TotalDietaryTags = totalDietaryTags
            };
        }

        public async Task<List<CategoryDistributionDTO>> GetCategoryDistributionAsync()
        {
            var commodities = await _commodityRepo.GetAllCommoditiesAsync();

            var grouped = commodities
                .Where(c => !string.IsNullOrEmpty(c.Category))
                .GroupBy(c => c.Category)
                .Select(g => new CategoryDistributionDTO
                {
                    Name = g.Key!,          
                    Value = g.Count()    
                })
                .ToList();

            return grouped;
        }

        public async Task<List<CategoryDistributionDTO>> GetDietaryTagDistributionAsync()
        {
            var productPrices = await _productDietaryTagRepo.GetAllProductsWithOptionalTagsAsync();

            // Flatten lahat ng tags at group by tag name
            var grouped = productPrices
                .SelectMany(pp => pp.ProductDietaryTags) // lahat ng tags
                .Where(pdt => pdt.DietaryTag != null)
                .GroupBy(pdt => pdt.DietaryTag.Name)
                .Select(g => new CategoryDistributionDTO
                {
                    Name = g.Key!,
                    Value = g.Count() // bilang ng ProductPrice na may tag na ito
                })
                .ToList();

            return grouped;
        }


        public async Task<List<PriceChangeDTO>> GetTop5PriceIncreasesAsync()
        {
            var all = await _productPriceRepo.GetPriceIncreasesAsync();

            return all
                .OrderByDescending(x =>
                {
                    if (decimal.TryParse(x.Change.TrimEnd('%', '+'), out var val))
                        return val;
                    return 0; // kapag "∞" or "N/A", treat as 0
                })
                .Take(5)
                .ToList();
        }

        public async Task<List<PriceChangeDTO>> GetTop5PriceDecreasesAsync()
        {
            var all = await _productPriceRepo.GetPriceDecreasesAsync();

            return all
                .OrderBy(x =>
                {
                    if (decimal.TryParse(x.Change.TrimEnd('%', '+'), out var val))
                        return val;
                    return 0;
                })
                .Take(5)
                .ToList();
        }


    }
}
