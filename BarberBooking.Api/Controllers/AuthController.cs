using Microsoft.AspNetCore.Mvc;
using BarberBooking.Core.DTOs;
using BarberBooking.Core.Services;

namespace BarberBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.LoginAsync(request);
            if (response == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Apenas clientes podem se registrar automaticamente
            if (request.UserType != "Client")
                return BadRequest(new { message = "Only clients can self-register. Employees must be created by admin." });

            var response = await _authService.RegisterAsync(request);
            if (response == null)
                return BadRequest(new { message = "Email already registered or registration failed" });

            return CreatedAtAction(nameof(Register), response);
        }
    }
}
