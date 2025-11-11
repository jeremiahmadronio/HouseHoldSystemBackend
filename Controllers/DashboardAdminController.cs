using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;

using WebApplication2.dto.ProductPriceDTO;


namespace WebApplication2.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DashboardAdminController : ControllerBase

    {

        private readonly DashboardService _dashboardService;
        public DashboardAdminController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _dashboardService.GetDashboardSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("category-distribution")]
        public async Task<IActionResult> GetCategoryDistribution()
        {
            var result = await _dashboardService.GetCategoryDistributionAsync();
            return Ok(result);
        }

        [HttpGet("dietary-tag-distribution")]
        public async Task<IActionResult> GetDietaryTagDistribution()
        {
            var data = await _dashboardService.GetDietaryTagDistributionAsync();
            return Ok(data);
        }

        [HttpGet("price-increases")]
        public async Task<IActionResult> GetPriceIncreases()
        {
            var data = await _dashboardService.GetTop5PriceIncreasesAsync();
            return Ok(data);
        }

        [HttpGet("price-decreases")]
        public async Task<IActionResult> GetPriceDecreases()
        {
            var data = await _dashboardService.GetTop5PriceDecreasesAsync();
            return Ok(data);
        }

    }
}
