using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;

using WebApplication2.dto.GamesDTO;

namespace WebApplication2.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase {


        private readonly IGamesService _gamesService;

        public GamesController(IGamesService gamesService) { 
        
        this._gamesService = gamesService;
                }


        //add games
        [HttpPost("AddGames")]
        public IActionResult Add([FromBody] AddGamesDTO dto)
        {
            if (_gamesService.createGames(dto, out string message))
                return Ok(new { success = true, message });

            return BadRequest(new { success = false, message });
        }


        //display game
        [HttpGet("displayGames")]
        public IActionResult display() {
            
            var games = _gamesService.displayGames();
            return Ok(games);
        
        }

        [HttpPost("checkAnswer")]
        public IActionResult submitAnswer([FromBody] SubmitAnswerDTO dto) {

            if (_gamesService.checkAnswer(dto, out string message))
                return Ok(new { success = true, message });

            return Ok(new { success = false, message });
        }



    }


}