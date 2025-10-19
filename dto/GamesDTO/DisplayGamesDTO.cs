namespace WebApplication2.dto.GamesDTO
{
    public class DisplayGamesDTO
    {
        public required Guid gameId { get; set; }
        public string? photo { get; set; }
        public required string product_name { get; set; }
        public required int mock_price { get; set; }
        public required string unit { get; set; }
        public required int correct_price { get; set; }

    }

}
