using WebApplication2.data;
using WebApplication2.models;
using WebApplication2.dto.ProductPriceDTO;
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
            // Kukunin lahat ng prices (hindi lang 2)
            return await _context.ProductPrices
                .Where(p => p.CommodityId == commodityId)
                .OrderByDescending(p => p.DateReported)
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




        public async Task<List<PriceChangeDTO>> GetPriceIncreasesAsync()
        {
            var commodities = await _context.Commodities
                .Include(c => c.Prices)
                .ToListAsync();

            var result = new List<PriceChangeDTO>();

            foreach (var commodity in commodities)
            {
                var latestTwo = commodity.Prices
                    .OrderByDescending(p => p.DateReported)
                    .Take(2)
                    .ToList();

                if (latestTwo.Count == 2)
                {
                    var latest = latestTwo[0];
                    var previous = latestTwo[1];
                    if (latest.Price > previous.Price)
                    {
                        if (previous.Price == 0) // ✅ prevent divide by zero
                        {
                            result.Add(new PriceChangeDTO
                            {
                                Name = commodity.ProductName,
                                Old = previous.Price,
                                New = latest.Price,
                                Change = "+∞%" // or pwede mong ilagay “N/A”
                            });
                        }
                        else
                        {
                            var changePercent = ((latest.Price - previous.Price) / previous.Price) * 100;
                            result.Add(new PriceChangeDTO
                            {
                                Name = commodity.ProductName,
                                Old = previous.Price,
                                New = latest.Price,
                                Change = $"+{changePercent:F2}%"
                            });
                        }
                    }

                }
            }

            return result;
        }

        // 🟥 DECREASES
        public async Task<List<PriceChangeDTO>> GetPriceDecreasesAsync()
        {
            var commodities = await _context.Commodities
                .Include(c => c.Prices)
                .ToListAsync();

            var result = new List<PriceChangeDTO>();

            foreach (var commodity in commodities)
            {
                var latestTwo = commodity.Prices
                    .OrderByDescending(p => p.DateReported)
                    .Take(2)
                    .ToList();

                if (latestTwo.Count == 2)
                {
                    var latest = latestTwo[0];
                    var previous = latestTwo[1];

                    if (latest.Price < previous.Price)
                    {
                        if (previous.Price == 0)
                        {
                            result.Add(new PriceChangeDTO
                            {
                                Name = commodity.ProductName,
                                Old = previous.Price,
                                New = latest.Price,
                                Change = "N/A"
                            });
                        }
                        else
                        {
                            var changePercent = ((latest.Price - previous.Price) / previous.Price) * 100;
                            result.Add(new PriceChangeDTO
                            {
                                Name = commodity.ProductName,
                                Old = previous.Price,
                                New = latest.Price,
                                Change = $"{changePercent:F2}%"
                            });
                        }
                    }

                }
            }

            return result;
        }
    }
}