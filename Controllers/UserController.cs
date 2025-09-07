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
        //login
        [HttpPost("login")]
        public IActionResult Login( [FromBody] LoginDTO request)
        {
            bool success = _userService.Login(request);

            if (!success)
                return Unauthorized("Invalid email or password");

            return Ok("Login successful");

        }


        //createuser    username password email
        [HttpPost("create-user")]

        public IActionResult createUser([FromBody] CreateUserDTO request)
        {
            bool success = _userService.CreateUser(request, out string message);

            if (!success) return BadRequest(message);

            return Ok(message);

        }

    }
}