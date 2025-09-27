
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models
{

    public class Games
    {

        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        public  byte[]? photo { get; set; }
        public required string product_name { get; set; }
        public required int correct_price { get; set; }
        public required string unit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}