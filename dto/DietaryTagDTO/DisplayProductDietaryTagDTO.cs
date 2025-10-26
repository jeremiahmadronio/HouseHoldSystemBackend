namespace WebApplication2.dto.DietaryTagDTO
{
    public class DisplayProductDietaryTagDTO
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal LatestPrice { get; set; }
        public List<string> DietaryTags { get; set; }
    }
}
