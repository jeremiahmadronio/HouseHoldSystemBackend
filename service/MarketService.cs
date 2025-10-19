using WebApplication2.repositories;
using WebApplication2.models;
using WebApplication2.dto.MarketDTO;


namespace WebApplication2.service

{
    public class MarketService
    {
        private readonly IMarketRepository _repo;

        public MarketService(IMarketRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<DisplayMarketDTO>> GetAllMarketsAsync()
        {
            var markets = await _repo.GetAllAsync();
            return markets.Select(m => new DisplayMarketDTO
            {
                MarketId = m.MarketId,
                MarketName = m.MarketName,
                Region = m.Region,
                IsActive = m.IsActive 
            });
        }

        public async Task<DisplayMarketDTO?> GetMarketByIdAsync(int id)
        {
            var market = await _repo.GetByIdAsync(id);
            if (market == null) return null;

            return new DisplayMarketDTO
            {
                MarketId = market.MarketId,
                MarketName = market.MarketName,
                Region = market.Region,
                IsActive = market.IsActive
            };
        }

        public async Task AddMarketAsync(CreateMarketDTO dto)
        {
            var newMarket = new Market
            {
                MarketName = dto.MarketName,
                Region = "NCR",
                IsActive = true
            };
            await _repo.AddAsync(newMarket);
        }

        public async Task UpdateMarketAsync(int id, DisplayMarketDTO dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Market not found");

            existing.MarketName = dto.MarketName;
            existing.Region = dto.Region;
            existing.IsActive = dto.IsActive ?? false ;

            await _repo.UpdateAsync(existing);
        }

        //  single or bulk delete logic
        public async Task DeleteMarketsAsync(IEnumerable<int> ids)
        {
            if (ids.Count() == 1)
            {
                await _repo.DeleteAsync(ids.First());
            }
            else
            {
                await _repo.DeleteBulkAsync(ids);
            }
        }

        public async Task<bool> DeleteMarketAsync(int id)
        {
            var market = await _repo.GetByIdAsync(id);
            if (market == null)
                return false;

            await _repo.DeleteAsync(market.MarketId);
            return true;
        }




    }
}