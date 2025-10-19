using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;
namespace WebApplication2.repositories.repository

{     public class MarketRepository : IMarketRepository
    {
        private readonly ApplicationDbContext _context;

        public MarketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Market?> GetByIdAsync(int id)
        {
            return await _context.Markets.FirstOrDefaultAsync(m => m.MarketId == id);
        }
        //get all
        public async Task<IEnumerable<Market>> GetAllAsync()
        {
            return await _context.Markets.ToListAsync();
        }

        //add
        public async Task AddAsync(Market market)
        {
            _context.Markets.Add(market);
            await _context.SaveChangesAsync();
        }

        //update
        public async Task UpdateAsync(Market market)
        {
            _context.Markets.Update(market);
            await _context.SaveChangesAsync();
        }

        //delete
        public async Task DeleteAsync(int id)
        {
            var market = await _context.Markets.FindAsync(id);
            if (market != null)
            {
                _context.Markets.Remove(market);
                await _context.SaveChangesAsync();
            }
        }

        //delete bulk
        public async Task DeleteBulkAsync(IEnumerable<int> ids)
        {
            var markets = await _context.Markets
                .Where(m => ids.Contains(m.MarketId))
                .ToListAsync();

            if (markets.Any())
            {
                _context.Markets.RemoveRange(markets);
                await _context.SaveChangesAsync();
            }
        }
    }
}