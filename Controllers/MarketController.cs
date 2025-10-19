using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;

using WebApplication2.dto.MarketDTO;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketController : ControllerBase
    {

        private readonly MarketService _marketService;

        public MarketController(MarketService marketService)
        {               
            _marketService = marketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var markets = await _marketService.GetAllMarketsAsync();
            return Ok(markets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var market = await _marketService.GetMarketByIdAsync(id);
            if (market == null)
                return NotFound();

            return Ok(market);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateMarketDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MarketName))
                return BadRequest("Market name is required.");

            await _marketService.AddMarketAsync(dto);
            return Ok(new { message = "Market added successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DisplayMarketDTO dto)
        {
            await _marketService.UpdateMarketAsync(id, dto);
            return Ok(new { message = "Market updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _marketService.DeleteMarketAsync(id);
            return Ok(new { message = "Market deleted successfully" });
        }


        [HttpPost("bulk")]
        public async Task<IActionResult> AddBulk([FromBody] List<CreateMarketDTO> dtoList)
        {
            if (dtoList == null || dtoList.Count == 0)
                return BadRequest("No markets provided.");

            foreach (var dto in dtoList)
            {
                if (!string.IsNullOrWhiteSpace(dto.MarketName))
                    await _marketService.AddMarketAsync(dto);
            }

            return Ok(new { message = $"{dtoList.Count} markets added successfully" });
        }
    }
}