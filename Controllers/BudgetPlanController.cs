using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.dto.BudgetPlanDTO;
using WebApplication2.service;
using WebApplication2.data;
using System;
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetPlanController : ControllerBase
    {
        private readonly BudgetPlanService _budgetPlanService;
        private readonly ApplicationDbContext _context;

        public BudgetPlanController(BudgetPlanService budgetPlanService, ApplicationDbContext context)
        {
            _budgetPlanService = budgetPlanService;
            _context = context;
        }

       [HttpPost("generate")]
public async Task<IActionResult> GenerateBudgetPlan(Guid userId, [FromBody] BudgetPlanRequestDTO request)
{
    if (request == null)
        return BadRequest("Invalid request");

    try
    {
        var allCommodities = await _context.Commodities
            .Include(c => c.Prices)
            .ToListAsync();

        var result = await _budgetPlanService.GenerateBudgetPlanAsync(userId, request, allCommodities);

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}


        [HttpGet("commodities/by-tag")]
        public async Task<IActionResult> GetCommoditiesByTag([FromQuery] string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return BadRequest(new { message = "Tag is required." });

            try
            {
                var commodities = await _budgetPlanService.GetCommoditiesByDietaryTagAsync(tag);

                if (commodities == null || commodities.Count == 0)
                {
                    return Ok(new
                    {
                        commodities = new List<object>(),
                        message = "No commodities found for the selected dietary tag."
                    });
                }

                return Ok(new
                {
                    commodities = commodities,
                    message = "Commodities retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server error: " + ex.Message });
            }
        }






    }
}
