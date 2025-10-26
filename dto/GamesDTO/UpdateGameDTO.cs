namespace WebApplication2.dto.GamesDTO
{
    public class UpdateGameDTO
    {

        public Guid id { get; set; }
        public string product_name { get; set; } = string.Empty;
        public int correct_price { get; set; }
        public string unit { get; set; } = string.Empty;
        public string? photo { get; set; }

    }

}
