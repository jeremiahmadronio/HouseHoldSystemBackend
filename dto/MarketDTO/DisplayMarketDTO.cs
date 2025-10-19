namespace WebApplication2.dto.MarketDTO
{
  
    public class DisplayMarketDTO
    {
        public required int MarketId { get; set; }
        public required string MarketName { get; set; }
        public required string Region { get; set; }
        public bool? IsActive { get; set; }
    }
}
