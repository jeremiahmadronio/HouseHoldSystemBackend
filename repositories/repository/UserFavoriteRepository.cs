using WebApplication2.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.data;

namespace WebApplication2.repositories
{
    public class UserFavoriteRepository : IUserFavoriteRepository
    {
        private readonly ApplicationDbContext _context;

        public UserFavoriteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Commodity>> GetUserFavoritesAsync(Guid userId)
        {
            return await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Commodity)
                .Select(f => f.Commodity)
                .ToListAsync();
        }

        public async Task<UserFavorite> AddFavoriteAsync(Guid userId, int commodityId)
        {
            // Check kung naka-favorite na
            if (await IsFavoriteAsync(userId, commodityId))
                return null;

            var favorite = new UserFavorite
            {
                UserId = userId,
                CommodityId = commodityId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFavorites.Add(favorite);
            await _context.SaveChangesAsync();
            return favorite;
        }

        public async Task<bool> RemoveFavoriteAsync(Guid userId, int commodityId)
        {
            var favorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CommodityId == commodityId);

            if (favorite == null)
                return false;

            _context.UserFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> IsFavoriteAsync(Guid userId, int commodityId)
        {
            return await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.CommodityId == commodityId);
        }
    }
}
