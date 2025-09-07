using System.ComponentModel.DataAnnotations;
namespace WebApplication2.models
{
    public class User
    {

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required String username { get; set; }
        public required String password { get; set; }

        public required String email { get; set; }


    }
}
