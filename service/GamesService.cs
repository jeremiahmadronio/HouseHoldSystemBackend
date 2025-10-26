using WebApplication2.dto.GamesDTO;
using WebApplication2.repositories;
using WebApplication2.models;

namespace WebApplication2.service
{
    public class GamesService : IGamesService
    {
        private readonly IGamesRepository _gameRepository;
        private readonly IUserRepository _userRepository;
        private readonly Random _random = new Random();


        public GamesService(IGamesRepository gameRepository, IUserRepository userRepository )
        {
            _gameRepository = gameRepository;
            _userRepository = userRepository;
        }
        //create games
        public bool createGames(AddGamesDTO dto, out string message)
        {
            try
            {
                byte[]? imageBytes = null;

                if (!string.IsNullOrEmpty(dto.photo))
                {
                    // If may "data:image/png;base64," part, remove it
                    var base64Data = dto.photo.Contains(",")
                        ? dto.photo.Split(',')[1]
                        : dto.photo;

                    imageBytes = Convert.FromBase64String(base64Data);
                }

                var game = new Games
                {
                    photo = imageBytes,
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



        //display games
        public List<DisplayGamesDTO> displayGames() { 

            var display = _gameRepository.GetAllGames();

            var displayList = display.Select(d => new DisplayGamesDTO {

                gameId = d.id,
                photo = d.photo != null ? Convert.ToBase64String(d.photo) : string.Empty,
                product_name = d.product_name,
                unit = d.unit,
                mock_price = GenerateMockPrice(d.correct_price),
                correct_price = d.correct_price,
            }).ToList();

            return displayList;
        
        }

        //generate mock price
        private int GenerateMockPrice(decimal realPrice)
        {
            if (_random.NextDouble() < 0.3)
            {
                return (int)realPrice;
            }

            int min = Math.Max(0, (int)realPrice - 50);
            int max = (int)realPrice + 100;

            return _random.Next(min, max + 1);
        }



        //Cheking the answer if correct
        public bool checkAnswer(SubmitAnswerDTO answerDTO, out string message) {

            var answer = _gameRepository.getGamesById(answerDTO.gameId);

            if (answer == null) {

                message = "game not found";
                return false;
            
            }
            bool isFair = answerDTO.mock_answer == answer.correct_price;

            bool userCorrect = (answerDTO.UserChoice == "Fair" && isFair) || (answerDTO.UserChoice == "Fake" && !isFair);

            message = userCorrect ? "Correct" : "Wrong";

            return userCorrect;

        
        }

        public object getTotalUserAndGames() { 
            var totalUser =  _userRepository.GetTotalUser();
            var totalGames = _gameRepository.GetTotalGames();

            return new
            {

                totalGames,
                totalUser,
            };
        
        }


        public bool updateGame(UpdateGameDTO dto, out string message)
        {
            var existing = _gameRepository.getGamesById(dto.id);
            if (existing == null)
            {
                message = "Game not found.";
                return false;
            }

            existing.product_name = dto.product_name;
            existing.correct_price = dto.correct_price;
            existing.unit = dto.unit;

            if (!string.IsNullOrEmpty(dto.photo))
            {
                existing.photo = Convert.FromBase64String(
                    dto.photo.Replace("data:image/jpeg;base64,", "")
                             .Replace("data:image/png;base64,", "")
                             .Replace("data:image/webp;base64,", "")
                );
            }

            _gameRepository.UpdateGame(existing);
            message = "Game updated successfully.";
            return true;
        }


        public bool DeleteGame(Guid id, out string message)
        {
            var existing = _gameRepository.getGamesById(id);
            if (existing == null)
            {
                message = "Game not found.";
                return false;
            }

            _gameRepository.DeleteGames(id);
            message = "Game deleted successfully.";
            return true;
        }

    }
}
