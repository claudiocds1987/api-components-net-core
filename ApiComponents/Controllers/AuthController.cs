using ApiComponents.Application.Features.Auth.Commands.Login;
using ApiComponents.Application.Features.Auth.Commands.Register;
using ApiComponents.Application.Features.Auth.Queries.GetUserByUsername;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        return Ok(await sender.Send(command));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        return Ok(await sender.Send(command));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var username = User.Identity?.Name;
        // La autorización debería asegurar que Identity.Name no es nulo,
        // pero por si acaso, lo comprobamos rápidamente
        if (string.IsNullOrEmpty(username)) return Unauthorized();

        var query = new GetUserByUsernameQuery { username = username };
        return Ok(await sender.Send(query));
    }
}