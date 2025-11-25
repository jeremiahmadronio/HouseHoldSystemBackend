namespace WebApplication2.dto.ProductPriceDTO
{
    public class DisplayProductInfoDTO
    {
        public int CommodityId { get; set; }
        public string ProductName { get; set; }
        public decimal LatestPrice { get; set; }
        public string Unit { get; set; }
        public string LocalName { get; set; }
        public string Category { get; set; }
        public DateTime DateReported { get; set; }
        public string? PercentageChange { get; set; }
        public bool IsFavorite { get; set; } = false; // default false


    }
}
