using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;
namespace WebApplication2.repositories.repository
{

    public class ProductDietaryTagRepository : IProductDietaryTagRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductDietaryTagRepository(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<IEnumerable<ProductPrice>> GetAllProductsWithOptionalTagsAsync()
        {
            // Kunin yung latest ReportId (pinaka-latest na batch)
            var latestReportId = await _context.ProductPrices
                .MaxAsync(pp => pp.ReportId);

            return await _context.ProductPrices
                .Where(pp => pp.ReportId == latestReportId) // filter sa latest batch
                .Include(pp => pp.Commodity)
                .Include(pp => pp.ProductDietaryTags)
                    .ThenInclude(pdt => pdt.DietaryTag)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductPrice>> GetAllProductPricesByCommodityAsync(int commodityId)
        {
            return await _context.ProductPrices
                .Where(pp => pp.CommodityId == commodityId)
                .Include(pp => pp.ProductDietaryTags)
                .ToListAsync();
        }



        public async Task<ProductPrice> GetProductPriceWithTagsAsync(int productPriceId)
        {
            return await _context.ProductPrices
                .Include(pp => pp.Commodity)
                .Include(pp => pp.ProductDietaryTags)
                    .ThenInclude(pdt => pdt.DietaryTag)
                .FirstOrDefaultAsync(pp => pp.ProductPriceId == productPriceId);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<int> GetTotalUniqueCommoditiesAsync()
        {
            return await _context.ProductPrices
                .Select(pp => pp.Commodity.ProductName)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetTotalCommoditiesWithTagsAsync()
        {
            return await _context.ProductPrices
                .Where(pp => pp.ProductDietaryTags.Any())
                .Select(pp => pp.Commodity.ProductName)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetTotalCommoditiesWithoutTagsAsync()
        {
            return await _context.ProductPrices
                .Where(pp => !pp.ProductDietaryTags.Any())
                .Select(pp => pp.Commodity.ProductName)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetTotalUniqueTagsAsync()
        {
            return await _context.DietaryTags
                .Select(dt => dt.Name)
                .Distinct()
                .CountAsync();
        }


    }

}