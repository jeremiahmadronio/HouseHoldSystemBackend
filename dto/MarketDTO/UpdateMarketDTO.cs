namespace WebApplication2.dto.MarketDTO
{
    public class UpdateMarketDTO
    {
        public string MarketName { get; set; }
        public string MarketDescription { get; set; }
        public string Region { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public decimal Ratings { get; set; }
        public bool IsActive { get; set; }
    }
}
