// Inside WebApplication2.dto.userDTO
namespace WebApplication2.dto.userDTO
{
    public class UserProfileByIdDTO
    {
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}