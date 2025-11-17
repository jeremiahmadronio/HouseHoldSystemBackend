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

        // GET: api/Market
        [HttpGet("display-market")]
        public async Task<IActionResult> GetAll()
        {
            var markets = await _marketService.GetAllMarketsAsync();
            return Ok(markets);
        }

        // GET: api/Market/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var market = await _marketService.GetMarketByIdAsync(id);
            if (market == null)
                return NotFound();

            return Ok(market);
        }

        // POST: api/Market
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateMarketDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MarketName))
                return BadRequest("Market name is required.");

            await _marketService.AddMarketAsync(dto);
            return Ok(new { message = "Market added successfully" });
        }

        // POST: api/Market/bulk
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

        // PUT: api/Market/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMarketDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MarketName))
                return BadRequest("Market name is required.");

            await _marketService.UpdateMarketAsync(id, dto);
            return Ok(new { message = "Market updated successfully" });
        }

        // DELETE: api/Market/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _marketService.DeleteMarketAsync(id);
            if (!deleted)
                return NotFound();

            return Ok(new { message = "Market deleted successfully" });
        }

        // DELETE: api/Market/bulk
        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteBulk([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest("No market IDs provided.");

            await _marketService.DeleteMarketsAsync(ids);
            return Ok(new { message = $"{ids.Count} markets deleted successfully" });
        }
    }
}
