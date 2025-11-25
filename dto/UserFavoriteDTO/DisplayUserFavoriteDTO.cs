namespace WebApplication2.dto.UserFavoriteDTO
{
    public class DisplayUserFavoriteDTO
    {
        public int CommodityId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string LocalName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal LatestPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public string? PercentageChange { get; set; }
    }
}
