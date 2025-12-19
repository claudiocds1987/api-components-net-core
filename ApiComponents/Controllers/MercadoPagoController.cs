using ApiComponents.DTOs;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MercadoPagoController : ControllerBase
{
    private readonly IMercadoPagoService _mpService;

    public MercadoPagoController(IMercadoPagoService mpService)
    {
        _mpService = mpService;
    }

    [HttpPost("create-preference")]
    public async Task<IActionResult> CreatePreference([FromBody] CartDto cart)
    {
        var preferenceId = await _mpService.CreatePreferenceAsync(cart);
        return Ok(new { id = preferenceId });
    }
}