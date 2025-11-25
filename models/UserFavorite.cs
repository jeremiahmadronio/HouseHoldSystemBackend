using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.models
{
    public class UserFavorite
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to User
        public Guid UserId { get; set; }
        public User User { get; set; }

        // Foreign key to Commodity
        public int CommodityId { get; set; }
        public Commodity Commodity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
