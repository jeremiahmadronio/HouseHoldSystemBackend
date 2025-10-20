using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;


namespace WebApplication2.repositories.repository
{
    public class ProductPriceRepository : IProductPriceRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductPriceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ProductPrice price)
        {
            _context.ProductPrices.Add(price);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }



        public async Task<ProductPrice> GetByCommodityAndReportAsync(int commodityId, int reportId)
        {
            return await _context.ProductPrices
                .FirstOrDefaultAsync(p => p.CommodityId == commodityId && p.ReportId == reportId);
        }

        public async Task UpdateAsync(ProductPrice price)
        {
            _context.ProductPrices.Update(price);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductPrice?> GetLatestByCommodityAsync(int commodityId)
        {
            return await _context.ProductPrices
                .Where(p => p.CommodityId == commodityId)
                .OrderByDescending(p => p.DateReported)
                .FirstOrDefaultAsync();
        }


        public async Task<List<ProductPrice>> GetLatestTwoByCommodityAsync(int commodityId)
        {
            return await _context.ProductPrices
                .Where(p => p.CommodityId == commodityId)
                .OrderByDescending(p => p.DateReported)
                .Take(2)
                .ToListAsync();
        }

        public async Task DeleteByCommodityIdAsync(int commodityId)
        {
            var prices = await _context.ProductPrices
                .Where(p => p.CommodityId == commodityId)
                .ToListAsync();

            if (prices.Any())
            {
                _context.ProductPrices.RemoveRange(prices);
                await _context.SaveChangesAsync();
            }
        }



    }
}