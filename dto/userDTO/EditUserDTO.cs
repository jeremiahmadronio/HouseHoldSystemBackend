namespace WebApplication2.dto.userDTO
{
    public class EditUserDTO
    {
        public required string Username { get; set; }  
        public required string Password { get; set; }
        public required string Email { get; set; }    
        public string? Phone { get; set; }           
    }
}
