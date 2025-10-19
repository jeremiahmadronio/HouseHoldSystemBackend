namespace WebApplication2.dto.userDTO
{
    public class GetUserDTO
    {


        public required String username { get; set; }
        public required String password { get; set; }

        public String? email { get; set; }
        public String? phone { get; set; }
        public byte[]? photo { get; set; }

        public DateTime CreatedAt { get; set; } 
    }
}



