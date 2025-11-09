using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;



namespace WebApplication2.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CommodityController : ControllerBase
    {


        private readonly CommoditiyService _commoditiyService;

        public CommodityController(CommoditiyService commoditiyService)
        {

            _commoditiyService = commoditiyService;
        }




        [HttpGet("categories")]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            var categories = await _commoditiyService.GetAllCategoriesAsync();
            return Ok(categories);
        }


    }



}