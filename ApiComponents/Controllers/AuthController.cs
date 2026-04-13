using ApiComponents.DTOs;
using ApiComponents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await authService.Register(registerDto);
        if (result == null)
            return BadRequest(new { message = "El usuario o el correo ya están registrados." });

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.Login(loginDto.username, loginDto.password);
        if (result == null)
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return Unauthorized();

        // El servicio ya devuelve el UserResponseDto
        var userDto = await authService.GetUserByUsernameAsync(username);

        if (userDto == null) return NotFound();

        return Ok(userDto);
    }
}