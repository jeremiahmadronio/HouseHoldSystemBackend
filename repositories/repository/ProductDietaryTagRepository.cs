using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WebApplication2.repositories.repository
{
    public class ProductDietaryTagRepository : IProductDietaryTagRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductDietaryTagRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Commodity>> GetAllCommoditiesWithOptionalTagsAsync()
        {
            return await _context.Commodities
                .Include(c => c.ProductDietaryTags)
                    .ThenInclude(pdt => pdt.DietaryTag)
                .Include(c => c.Prices)
                .ToListAsync();
        }

        public async Task<Commodity> GetCommodityWithTagsAsync(int commodityId)
        {
            return await _context.Commodities
                .Include(c => c.ProductDietaryTags)
                    .ThenInclude(pdt => pdt.DietaryTag)
                .Include(c => c.Prices)
                .FirstOrDefaultAsync(c => c.CommodityId == commodityId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalUniqueCommoditiesAsync()
        {
            return await _context.Commodities.CountAsync();
        }

        public async Task<int> GetTotalCommoditiesWithTagsAsync()
        {
            return await _context.Commodities
                .Where(c => c.ProductDietaryTags.Any())
                .CountAsync();
        }

        public async Task<int> GetTotalCommoditiesWithoutTagsAsync()
        {
            return await _context.Commodities
                .Where(c => !c.ProductDietaryTags.Any())
                .CountAsync();
        }

        public async Task<int> GetTotalUniqueTagsAsync()
        {
            return await _context.DietaryTags.CountAsync();
        }
    }
}
