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

        // GET ALL
        public async Task<IEnumerable<DisplayMarketDTO>> GetAllMarketsAsync()
        {
            var markets = await _repo.GetAllAsync();
            return markets.Select(m => new DisplayMarketDTO
            {
                MarketId = m.MarketId,
                MarketName = m.MarketName,
                MarketDescription = m.MarketDescription,
                Region = m.Region,
                OpeningTime = m.OpeningTime.ToString(@"hh\:mm"),
                ClosingTime = m.ClosingTime.ToString(@"hh\:mm"),
                Ratings = m.ratings,
                IsActive = m.IsActive
            });
        }

        // GET BY ID
        public async Task<DisplayMarketDTO?> GetMarketByIdAsync(int id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return null;

            return new DisplayMarketDTO
            {
                MarketId = m.MarketId,
                MarketName = m.MarketName,
                MarketDescription = m.MarketDescription,
                Region = m.Region,
                OpeningTime = m.OpeningTime.ToString(@"hh\:mm"),
                ClosingTime = m.ClosingTime.ToString(@"hh\:mm"),
                Ratings = m.ratings,
                IsActive = m.IsActive
            };
        }

        // ADD
        public async Task AddMarketAsync(CreateMarketDTO dto)
        {
            var market = new Market
            {
                MarketName = dto.MarketName,
                MarketDescription = dto.MarketDescription,
                Region = dto.Region,
                OpeningTime = dto.OpeningTime,
                ClosingTime = dto.ClosingTime,
                ratings = dto.Ratings,
                IsActive = true
            };

            await _repo.AddAsync(market);
        }

        // UPDATE
        public async Task UpdateMarketAsync(int id, UpdateMarketDTO dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Market not found");

            existing.MarketName = dto.MarketName;
            existing.MarketDescription = dto.MarketDescription;
            existing.Region = dto.Region;
            existing.OpeningTime = dto.OpeningTime;
            existing.ClosingTime = dto.ClosingTime;
            existing.ratings = dto.Ratings;
            existing.IsActive = dto.IsActive;

            await _repo.UpdateAsync(existing);
        }

        // DELETE SINGLE
        public async Task<bool> DeleteMarketAsync(int id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }

        // DELETE BULK
        public async Task DeleteMarketsAsync(IEnumerable<int> ids)
        {
            if (ids.Count() == 1)
                await _repo.DeleteAsync(ids.First());
            else
                await _repo.DeleteBulkAsync(ids);
        }
    }
}
