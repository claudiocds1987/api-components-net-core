
using ApiComponents.DTOs;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MercadoPagoController : ControllerBase
{
    private readonly IMercadoPagoService _mpService;
    private readonly IOrderRepository _orderRepository;

    public MercadoPagoController(IMercadoPagoService mpService, IOrderRepository orderRepository)
    {
        _mpService = mpService;
        _orderRepository = orderRepository;
    }

    [HttpPost("create-preference")]
    public async Task<IActionResult> CreatePreference([FromBody] CartDto cart)
    {
        var preferenceId = await _mpService.CreatePreferenceAsync(cart);
        return Ok(new { id = preferenceId });
    }

    // WEBHOOK PROFESIONAL: Recibe notificaciones asincrónicas de MP
    [HttpPost("webhook")]
    public async Task<IActionResult> MercadoPagoWebhook([FromQuery] string topic, [FromQuery] string id)
    {
        if (topic == "payment")
        {
            var status = await _mpService.GetPaymentStatusAsync(id);
            // Aquí deberías buscar la orden y actualizarla
            // Para eso, el 'external_reference' en la preferencia es clave
        }
        return Ok();
    }

    // CONFIRMACIÓN SEGURA DESDE EL FRONTEND
    [HttpPost("confirm-payment")]
    public async Task<IActionResult> ConfirmPayment([FromBody] MercadoPagoConfirmationDto confirmation)
    {
        // Validamos el estado real del pago en los servidores de MP antes de actualizar nuestra DB
        var realStatus = await _mpService.GetPaymentStatusAsync(confirmation.PaymentId);

        if (realStatus == "approved")
        {
            await _orderRepository.UpdateStatusAsync(confirmation.PreferenceId, "Approved");
            return Ok(new { message = "Pago verificado y aprobado" });
        }

        return BadRequest("El pago no pudo ser verificado");
    }
}
