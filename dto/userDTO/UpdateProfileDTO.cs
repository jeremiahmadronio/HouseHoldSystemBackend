
namespace WebApplication2.dto.userDTO
{
    public class UpdateProfileDTO
    {
        public required Guid Id { get; set; } 

        
        public String? Username { get; set; }
        public String? Email { get; set; }
        public String? Phone { get; set; }

    
    }
}