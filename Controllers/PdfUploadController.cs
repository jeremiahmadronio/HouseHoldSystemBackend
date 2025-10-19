using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.dto.ProductPriceDTO;


namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfUploadController : ControllerBase
    {
        private readonly ProductsService _productsService;

        public PdfUploadController(ProductsService productsService)
        {
            _productsService = productsService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdfWithOCR(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Call service to process PDF with OCR
            var parsedData = await _productsService.ProcessPdfWithTabula(file);


            if (!parsedData.Any())
                return BadRequest("No data found in PDF (even with OCR).");

            // Save parsed data to DB
            var report = await _productsService.ProcessReportAsync(file.FileName, parsedData);

            return Ok(new
            {
                message = $"PDF processed successfully. {parsedData.Count} items saved.",
                reportId = report.ReportId
            });
        }



        [HttpGet("display")]
        public async Task<ActionResult<List<DisplayProductPriceDTO>>> GetAllProductPrices()
        {
            var result = await _productsService.GetAllProductPriceDisplayAsync();
            return Ok(result);
        }
    }
}
