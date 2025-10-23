using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;

using WebApplication2.dto.PriceReportDTO;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceReportController : ControllerBase
    {
        private readonly PriceReportService _priceRepo;

        public PriceReportController(PriceReportService priceReportService) {
            _priceRepo = priceReportService;
        }


   

        [HttpGet("getAll")]
        public async Task<IActionResult> getAllUser()
            => Ok(await _priceRepo.getAllAsync());

    }   
}