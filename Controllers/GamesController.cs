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



        [HttpPost("AddGames")]
        public IActionResult Add([FromBody] AddGamesDTO dto)
        {
            if (_gamesService.createGames(dto, out string message))
                return Ok(new { success = true, message });

            return BadRequest(new { success = false, message });
        }

    }


}