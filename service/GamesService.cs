using WebApplication2.dto.GamesDTO;
using WebApplication2.repositories;
using WebApplication2.models;

namespace WebApplication2.service
{
    public class GamesService : IGamesService
    {
        private readonly IGamesRepository _gameRepository;

        public GamesService(IGamesRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public bool createGames(AddGamesDTO dto, out string message)
        {
            try
            {
                var game = new Games
                {
                    photo = !string.IsNullOrEmpty(dto.photo)
                                ? Convert.FromBase64String(dto.photo)
                                : null,
                    product_name = dto.product_name,
                    correct_price = dto.correct_price,
                    unit = dto.unit
                };

                _gameRepository.AddGames(game);

                message = "Game added successfully";
                return true;
            }
            catch (FormatException)
            {
                message = "Photo field is not a valid Base64 string.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Failed to add game: {ex.Message}";
                return false;
            }
        }
    }
}
