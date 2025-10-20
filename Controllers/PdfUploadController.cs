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




        [HttpPost("add")]
        public async Task<IActionResult> AddNewProductPrice([FromBody] CreateProductDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                await _productsService.AddNewProductPriceAsync(dto);
                return Ok(new
                {
                    message = "Product price successfully added!",
                    product = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while adding product price.",
                    error = ex.Message
                });
            }
        }




        [HttpPut("edit/{commodityId}")]
        public async Task<IActionResult> EditProductPrice(int commodityId, [FromBody] EditProductDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid data.");

            var success = await _productsService.EditProductPriceByIdAsync(commodityId, dto);

            if (!success)
                return NotFound(new { message = "Product not found or update failed." });

            return Ok(new { message = "Product price successfully updated." });
        }



        [HttpDelete("delete/{commodityId}")]
        public async Task<IActionResult> DeleteProduct(int commodityId)
        {
            try
            {
                var result = await _productsService.DeleteProductByIdAsync(commodityId);
                if (result)
                    return Ok(new { message = "Product deleted successfully." });
                return NotFound(new { message = "Product not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
