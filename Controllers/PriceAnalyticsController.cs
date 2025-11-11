using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebApplication2.service;
using WebApplication2.dto.PriceAnalyticsDTO; // make sure ito ang namespace ng PriceFilterRequest

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceAnalyticsController : ControllerBase
    {
        private readonly PriceAnalyticsService _priceAnalyticsService;

        public PriceAnalyticsController(PriceAnalyticsService priceAnalyticsService)
        {
            _priceAnalyticsService = priceAnalyticsService;
        }

        
        [HttpPost("get")]
        public async Task<IActionResult> GetAnalytics([FromBody] PriceFilterRequest filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest("Filter data is required.");

                var result = await _priceAnalyticsService.GetAnalyticsAsync(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating analytics.", error = ex.Message });
            }
        }


        [HttpPost("weekly-chart")]
        public async Task<IActionResult> GetWeeklyChart([FromBody] PriceFilterRequest filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest("Filter data is required.");

                var result = await _priceAnalyticsService.GetWeeklyChartDataAsync(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating weekly chart data.", error = ex.Message });
            }
        }



    }
}
