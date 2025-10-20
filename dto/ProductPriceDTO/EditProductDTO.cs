namespace WebApplication2.dto.ProductPriceDTO
{
    public class EditProductDTO
    {
       
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public decimal LatestPrice { get; set; }
        public decimal? PreviousPrice { get; set; }
       
        public DateTime? LatestPriceDate { get; set; } = DateTime.UtcNow;
    }
}
