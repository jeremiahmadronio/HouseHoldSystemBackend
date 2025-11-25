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


    (bool Success, string? Role, Guid? UserId) Login(LoginDTO request);

    //createuser
    bool CreateUser(CreateUserDTO dto, out string message);

    bool CreateUsers(CreateUserAdminDTO dto, out string message);

    //resetPassword
    bool ChangePassword(Guid id, String currentPassword, String newPassword);

    //display user by email
    UserProfileDTO? GetUserProfile(string email);

    //update user info
    bool UpdateUserInfo(EditUserDTO dto, out string message);

    //delete user
    bool DeleteUserById(Guid id, out string message);

    // Change the signature to include Id (Guid)
    UserProfileResult? GetUserProfileById(Guid id);

    bool UpdateUserProfile(UpdateProfileDTO dto, out string message);





}
