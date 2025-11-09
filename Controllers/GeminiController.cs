using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;
using WebApplication2.AI_API_INTEGRATION;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public GeminiController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }


        [HttpGet("ask")]
        public async Task<IActionResult> Ask([FromQuery] string prompt)
        {
            var result = await _geminiService.GenerateTextAsync(prompt);
            return Ok(result);
        }
    }

}