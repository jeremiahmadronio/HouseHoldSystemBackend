using WebApplication2.models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication2.repositories
{
    public interface IUserFavoriteRepository
    {
        Task<IEnumerable<Commodity>> GetUserFavoritesAsync(Guid userId);
        Task<UserFavorite> AddFavoriteAsync(Guid userId, int commodityId);
        Task<bool> RemoveFavoriteAsync(Guid userId, int commodityId);
        Task<bool> IsFavoriteAsync(Guid userId, int commodityId);
    }
}
