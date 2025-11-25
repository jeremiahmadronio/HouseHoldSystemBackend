    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using WebApplication2.dto.UserFavoriteDTO;
    using WebApplication2.service;

    namespace WebApplication2.controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class FavoritesController : ControllerBase
        {
            private readonly UserFavoriteService _favoriteService;

            public FavoritesController(UserFavoriteService favoriteService)
            {
                _favoriteService = favoriteService;
            }


            [HttpPost("add")]
            public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteDTO dto)
            {
                var result = await _favoriteService.AddFavoriteAsync(dto);
                if (result == null)
                    return BadRequest("Commodity is already favorited");

                return Ok(result);
            }


            [HttpGet("{userId}")]
            public async Task<IActionResult> GetUserFavorites(Guid userId)
            {
                var favorites = await _favoriteService.GetUserFavoritesDisplayAsync(userId);
                return Ok(favorites);
            }


            [HttpDelete("remove")]
            public async Task<IActionResult> RemoveFavorite([FromBody] DeleteFavoriteDTO dto)
            {
                if (dto == null || dto.UserId == Guid.Empty || dto.CommodityId <= 0)
                    return BadRequest("Invalid request");

                var removed = await _favoriteService.RemoveFavoriteAsync(dto.UserId, dto.CommodityId);

                if (!removed)
                    return NotFound("Favorite not found");

                return Ok(new { message = "Favorite removed successfully" });
            }


        }
    }