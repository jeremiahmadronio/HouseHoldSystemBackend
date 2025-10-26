using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;

using WebApplication2.dto.DietaryTagDTO;

namespace WebApplication2.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductDietaryTagController : ControllerBase
    {


        private readonly ProductDietaryTagService _productDietaryService;

        public ProductDietaryTagController(ProductDietaryTagService productDietaryService)
        {

            _productDietaryService = productDietaryService;
        }


        [HttpGet("Display")]
        public async Task<IActionResult> GetAllProductsWithTags()
        {
            var data = await _productDietaryService.GetAllProductsWithTagsAsync();
            return Ok(data);
        }


        [HttpPut("{productPriceId}/tags")]
        public async Task<IActionResult> UpdateProductTags(int productPriceId, [FromBody] UpdateProductTagsDTO dto)
        {
            try
            {
                await _productDietaryService.UpdateProductTagsAsync(productPriceId, dto.TagIds);
                return Ok(new { message = "Tags updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("Stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _productDietaryService.GetDashboardStatsAsync();
            return Ok(new
            {
                totalProducts = stats.totalProducts,
                taggedProducts = stats.taggedProducts,
                untaggedProducts = stats.untaggedProducts,
                dietaryOptions = stats.totalTags
            });
        }



    }



}