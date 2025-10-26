using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models
{

    public class DietaryTag
    {
        [Key]
        public int DietaryTagId { get; set; }

        

        public string? Name { get; set; }
        public string? Description { get; set; }

        public ICollection<ProductDietaryTag> ProductDietaryTags { get; set; } = new List<ProductDietaryTag>();


    }

}