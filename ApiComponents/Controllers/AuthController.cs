using ApiComponents.DTOs;
using ApiComponents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.Register(registerDto);

            if (result == null)
            {
                return BadRequest(new { message = "El usuario o el correo ya están registrados." });
            }

            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.Login(loginDto.username, loginDto.password);

            if (result == null)
            {
                // Devolvemos 401 para que tu interceptor de Angular capture el error
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
            }

            return Ok(result);
        }
    }
}