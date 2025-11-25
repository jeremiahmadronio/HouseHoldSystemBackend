using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;

using WebApplication2.dto.userDTO;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;

        public UserController(IUserService userService) {           

            _userService = userService;
        }


        [HttpGet("getAll")]
        public async Task<IActionResult> getAllUser()
            => Ok(await _userService.getAllAsync());




        //data  email
        [HttpPost("verify-email")]
        public IActionResult VerifyEmail([FromBody] VerifyEmail request)
        {
          bool exists = _userService.verifyEmail(request.email);

            return Ok(new
            {
                Exists = exists,
                Message = exists ? "Email exists. You can proceed."
                            : "If your email exists, you can proceed."
            });


        }

        //Login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO request)
        {
            var (success, role, userId) = _userService.Login(request);

            if (!success)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(new
            {
                message = "Login successful",
                role = role ?? "Unknown",
                userId = userId // null kung admin (optional)
            });
        }


        //create User
        [HttpPost("create-user")]
        public IActionResult CreateUser([FromBody] CreateUserDTO request)
        {
            bool success = _userService.CreateUser(request, out string message);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }


        [HttpPost("create-users")]
        public IActionResult CreateUser([FromBody] CreateUserAdminDTO request)
        {
            bool success = _userService.CreateUsers(request, out string message);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }




        [HttpPost("change-password")] // Standard endpoint for this function
        public IActionResult ChangePassword([FromBody] ChangePasswordDTO request)
        {
            // Fix 1: Use the correct method name (ChangePassword)
            // Fix 2 & 3: Use PascalCase properties (Id, CurrentPassword, NewPassword)
            var success = _userService.ChangePassword(
                request.Id,              // ✅ FIX: Use 'Id' (PascalCase)
                request.CurrentPassword, // Assuming your DTO uses 'CurrentPassword'
                request.NewPassword      // Assuming your DTO uses 'NewPassword'
            );

            if (success)
                return Ok(new { message = "Password updated successfully" });

            // Assuming the service returns false if the old password was wrong or user not found
            return BadRequest(new { message = "Invalid current password or user not found" });
        }

        //display Settings
        [HttpGet("userProfile")]
        public ActionResult<UserProfileDTO> displayUserSettings([FromQuery] String email) { 
        
            var profile = _userService.GetUserProfile(email);

            if(profile == null)
                return NotFound("User not found");


            return Ok(profile);
                

        }

        [HttpPost("update-user")]
        public IActionResult UpdateUser([FromBody] EditUserDTO request)
        {
            bool success = _userService.UpdateUserInfo(request, out string message);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }


        [HttpDelete("delete-user")]
        // Change [FromQuery] string username to [FromQuery] Guid id
        public IActionResult DeleteUser([FromQuery] Guid id)
        {
            // Change DeleteUserByUsername to DeleteUserById
            bool success = _userService.DeleteUserById(id, out string message);

            if (!success)
                // Returns 404 Not Found if the ID does not exist
                return NotFound(new { message });

            return Ok(new { message });
        }



        // Inside WebApplication2.Controllers/UserController

        [HttpGet("userProfileById")]
        // Note: Changed return type from ActionResult<UserProfileByIdDTO> to ActionResult<UserProfileResult>
        public IActionResult DisplayUserProfileById([FromQuery] Guid id)
        {
            // Ensure the return type in the declaration matches the new type:
            var profile = _userService.GetUserProfileById(id);

            if (profile == null)
                return NotFound(new { message = "User not found" });

            return Ok(profile); // This will now serialize the explicit UserProfileResult record
        }


        // Inside WebApplication2.Controllers/UserController

        // ... (other methods)

        // ✅ NEW ENDPOINT: Update Profile by ID (Excludes Password Update)
        [HttpPut("update-profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDTO request)
        {
            // 1. Delegate business logic to the service layer
            bool success = _userService.UpdateUserProfile(request, out string message);

            // 2. Handle failure (User ID not found)
            if (!success)
                return NotFound(new { message }); // Returns 404 Not Found if user ID is invalid

            // 3. Handle success
            return Ok(new { message }); // Returns 200 OK
        }

    }
}