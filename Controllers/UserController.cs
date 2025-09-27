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
            var (success, role) = _userService.Login(request);

            if (!success)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(new
            {
                message = "Login successful",
                role = role ?? "Uknown"
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


        //reset-password
        [HttpPost("reset-password")]
        public IActionResult updateUser([FromBody] ResetPasswordDTO request) { 

            var success = _userService.ResetPassword(request.email , request.password);

            if (success)
                return Ok(new { message = "Password reset Successfully"});

            return BadRequest(new { message = "User not found" });
        }


        //display Settings
        [HttpGet("userProfile")]
        public ActionResult<UserProfileDTO> displayUserSettings([FromQuery] String email) { 
        
            var profile = _userService.GetUserProfile(email);

            if(profile == null)
                return NotFound("User not found");


            return Ok(profile);
                

        }
       

    }
}