using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models { 

	public class Commodity {
		[Key]
		public int CommodityId { get; set; }

		public required string ProductName { get; set; }

		public  string? Category { get; set; }
        public string? Specification { get; set; }

        public bool IsActive { get; set; } = true;
		public ICollection<ProductPrice> Prices { get; set; }


	}

}