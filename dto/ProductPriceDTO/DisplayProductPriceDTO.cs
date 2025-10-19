namespace WebApplication2.dto.ProductPriceDTO
{
    public class DisplayProductPriceDTO
    {
        public int id { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal LatestPrice { get; set; }
        public decimal? PreviousPrice { get; set; }
        public string Status { get; set; }
        public DateTime? LatestPriceDate { get; set; }
    }
}
