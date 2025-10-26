using System.ComponentModel.DataAnnotations;
using WebApplication2.models;


namespace WebApplication2.models
{

	public class ProductDietaryTag
	{
		[Key]
		public int Id { get; set; }



		public int? ProductPriceId { get; set; }
		public ProductPrice ProductPrice { get; set; }



		public int? DietaryTagId { get; set; }
		public DietaryTag DietaryTag { get; set; }







	}

}