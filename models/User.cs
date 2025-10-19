using System.ComponentModel.DataAnnotations;
namespace WebApplication2.models
{
    public class User
    {
        // Primary Key
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public required String username { get; set; } = string.Empty;
        public required String password { get; set; }

        public  String? email { get; set; }
        public  String? phone { get; set; }
        public byte[]? photo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
