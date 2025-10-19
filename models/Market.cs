using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models
{

    public class Market
    {
        [Key]
        public int MarketId { get; set; }
        public string MarketName { get; set; }
        public string Region { get; set; } = "NCR";
        public bool IsActive { get; set; } = true;
        public ICollection<ProductPrice> Prices { get; set; }
    }

}