using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models

{
    public class ProductPrice
    {
        [Key]
        public int ProductPriceId { get; set; }

        public int CommodityId { get; set; }
        public Commodity Commodity { get; set; }

        

        public decimal Price { get; set; }
        
        public string unit { get; set; }

        public int ReportId { get; set; }
        public PriceReport Report { get; set; }

        public DateTime DateReported { get; set; } = DateTime.UtcNow;

        public ICollection<ProductDietaryTag> ProductDietaryTags { get; set; } = new List<ProductDietaryTag>();
    }

}