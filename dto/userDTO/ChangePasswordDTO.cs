// ✅ FILE: WebApplication2/dto/userDTO/ChangePasswordDTO.cs (Rename/Refactor ResetPasswordDTO)

namespace WebApplication2.dto.userDTO
{
    public class ChangePasswordDTO // Renamed for clarity
    {
        public required Guid Id { get; set; }               // User ID for lookup
        public required string CurrentPassword { get; set; } // New field for verification
        public required string NewPassword { get; set; }     // The password to be saved
    }
}