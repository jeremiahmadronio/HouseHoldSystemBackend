using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;
namespace WebApplication2.repositories.repository
{
    public class CommodityRepository : ICommodityRepository
    {
        private readonly ApplicationDbContext _context;
        public CommodityRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Commodity?> GetByNameAsync(string productName)
        {
            return await _context.Commodities
                .FirstOrDefaultAsync(c => c.ProductName.ToLower() == productName.ToLower());
        }

        public async Task<Commodity> AddAsync(Commodity commodity)
        {
            _context.Commodities.Add(commodity);
            await _context.SaveChangesAsync();
            return commodity;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Commodity commodity)
        {
            _context.Commodities.Update(commodity);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Commodity>> GetAllCommoditiesAsync()
        {
            return await _context.Commodities
                                 .Include(c => c.Prices)
                                 .ToListAsync();
        }


        public async Task<Commodity?> GetByIdAsync(int id)
        {
            return await _context.Commodities
                .FirstOrDefaultAsync(c => c.CommodityId == id);
        }


        public async Task DeleteAsync(Commodity commodity)
        {
            _context.Commodities.Remove(commodity);
            await _context.SaveChangesAsync();
        }


        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _context.Commodities
                .Select(c => c.Category)
                .Distinct()
                .ToListAsync();
        }


        public async Task<List<Commodity>> GetAllCommoditiesByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Commodities
                                 .Where(c => ids.Contains(c.CommodityId))
                                 .ToListAsync();
        }



    }
}