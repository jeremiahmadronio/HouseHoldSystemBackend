namespace WebApplication2.dto.ProductPriceDTO
{
    public class CreateProductDTO
    {
       
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }    
        public DateTime? LatestPriceDate { get; set; } = DateTime.UtcNow;
    }
}
