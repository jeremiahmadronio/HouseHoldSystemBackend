using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

public record SendEmailRequest(string Email);
public record VerifyRequest(string Email, string Code);


namespace WebApplication2.settings
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly IMemoryCache _cache;

        public AuthController(EmailService emailService, IMemoryCache cache)
        {
            _emailService = emailService;
            _cache = cache;
        }



        [HttpPost("send-verification")]
        public IActionResult SendVerification([FromBody] SendEmailRequest req)
        {
            var newCode = new Random().Next(100000, 999999).ToString();

            _cache.Remove($"VERIF_{req.Email}");

            _cache.Set($"VERIF_{req.Email}", newCode, TimeSpan.FromMinutes(5));

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendVerificationCodeAsync(req.Email, newCode);
                }
                catch
                {
                }
            });

            return Ok(new { message = "Verification sent " });
        }


        [HttpPost("verify-code")]
        public IActionResult Verify([FromBody] VerifyRequest req)
        {
            if (_cache.TryGetValue($"VERIF_{req.Email}", out string? stored) && stored == req.Code)
            {
                _cache.Remove($"VERIF_{req.Email}");
                return Ok(new { verified = true });
            }
            return BadRequest(new { verified = false, message = "Invalid or expired code" });
        }
    }
}