using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;

using WebApplication2.dto.DietaryTagDTO;

namespace WebApplication2.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DietaryTagController : ControllerBase
    {


        private readonly DietaryTagService _dietaryTagService;

        public DietaryTagController(DietaryTagService dietaryTagService)
        {

            _dietaryTagService = dietaryTagService;
        }


        [HttpGet("Display")]
        public async Task<ActionResult<IEnumerable<DisplayDietaryTagDTO>>> GetAll()
        {
            var dietaryTags = await _dietaryTagService.GetAllDietaryTagsAsync();
            return Ok(dietaryTags);
        }



    }



}