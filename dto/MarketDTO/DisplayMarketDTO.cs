namespace WebApplication2.dto.MarketDTO
{
    public class DisplayMarketDTO
    {
        public int MarketId { get; set; }
        public string MarketName { get; set; }
        public string MarketDescription { get; set; }
        public string Region { get; set; }
        public string OpeningTime { get; set; }
        public string ClosingTime { get; set; }
        public decimal Ratings { get; set; }
        public bool IsActive { get; set; }
    }
}
