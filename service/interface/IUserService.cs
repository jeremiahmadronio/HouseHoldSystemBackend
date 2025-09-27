using WebApplication2.repositories;
using WebApplication2.dto.userDTO;

namespace WebApplication2.service;

public interface IUserService
{
    //get all user
    Task<IEnumerable<GetUserDTO>> getAllAsync();

    //verify email
    bool verifyEmail(string email);
    //login
    (bool Success, string? Role) Login(LoginDTO request);

    //createuser
    bool CreateUser(CreateUserDTO dto, out string message);

    //resetPassword
    bool ResetPassword(String email , String password);

    //display user by email
    UserProfileDTO? GetUserProfile(string email);

}
