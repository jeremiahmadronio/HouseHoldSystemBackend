
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models {

    public class Admin {

        [Key]
        public Guid id { get; set; } = Guid.NewGuid();
        public required string email { get; set; }
        public required string password { get; set; }
    }
}