using WebApplication2.models;
using WebApplication2.dto.UserFavoriteDTO;
using WebApplication2.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.service
{
	public class UserFavoriteService
	{
		private readonly IUserFavoriteRepository _repo;
		private readonly ICommodityRepository _commodityRepo;
		private readonly IProductPriceRepository _priceRepo;

		public UserFavoriteService(
			IUserFavoriteRepository repo,
			ICommodityRepository commodityRepo,
			IProductPriceRepository priceRepo)
		{
			_repo = repo;
			_commodityRepo = commodityRepo;
			_priceRepo = priceRepo;
		}

		// Add favorite
		public async Task<UserFavorite> AddFavoriteAsync(AddFavoriteDTO dto)
		{
			if (dto == null || dto.UserId == Guid.Empty || dto.CommodityId <= 0)
				return null;

			return await _repo.AddFavoriteAsync(dto.UserId, dto.CommodityId);
		}

		public async Task<List<DisplayUserFavoriteDTO>> GetUserFavoritesDisplayAsync(Guid userId)
		{
			var favorites = await _repo.GetUserFavoritesAsync(userId);
			var result = new List<DisplayUserFavoriteDTO>();

			foreach (var commodity in favorites)
			{
				var prices = await _priceRepo.GetLatestTwoByCommodityAsync(commodity.CommodityId);
				if (prices == null || prices.Count == 0) continue;

				var latest = prices.OrderByDescending(p => p.DateReported).First();
				var previous = prices.Count > 1 ? prices.OrderByDescending(p => p.DateReported).Skip(1).First() : null;

				string? percentageChange = null;
				if (previous != null)
				{
					if (previous.Price == 0)
						percentageChange = "+∞%";
					else
					{
						var changePercent = ((latest.Price - previous.Price) / previous.Price) * 100;
						percentageChange = changePercent >= 0 ? $"+{changePercent:F2}%" : $"{changePercent:F2}%";
					}
				}

				result.Add(new DisplayUserFavoriteDTO
				{
					CommodityId = commodity.CommodityId,
					ProductName = commodity.ProductName,
					LocalName = commodity.LocalName ?? "",
					Category = commodity.Category ?? "",
					LatestPrice = latest.Price,
					Unit = latest.unit,
					DateReported = latest.DateReported,
					PercentageChange = percentageChange
				});
			}

			return result;
		}

        //delete favorite
        public async Task<bool> RemoveFavoriteAsync(Guid userId, int commodityId)
        {
            if (userId == Guid.Empty || commodityId <= 0)
                return false;

            return await _repo.RemoveFavoriteAsync(userId, commodityId);
        }

    }
}
