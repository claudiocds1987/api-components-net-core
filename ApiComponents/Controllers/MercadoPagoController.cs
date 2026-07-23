using ApiComponents.Application.DTOs;
using ApiComponents.Application.Features.MercadoPago.Commands.CreatePreference;
using ApiComponents.Application.Features.MercadoPago.Commands.ProcessPaymentReturn;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MercadoPagoController : ControllerBase
    {
        private readonly ISender _sender;

        public MercadoPagoController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("create-preference")]
        public async Task<IActionResult> CreatePreference([FromBody] CartDto cart, CancellationToken cancellationToken)
        {
            var preferenceId = await _sender.Send(new CreatePreferenceCommand(cart), cancellationToken);
            return Ok(new { id = preferenceId });
        }

        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(
            [FromQuery] string preferenceId,
            [FromQuery] string paymentId,
            [FromQuery] string status,
            CancellationToken cancellationToken)
        {
            // Opcional: Esto también se podría delegar a un Command si tuviera lógica de base de datos
            Console.WriteLine($"[FRONTEND] Confirmación Recibida: Preference={preferenceId}, Status={status}");
            return Ok(new { message = "Confirmación procesada en el servidor." });
        }

        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn(
             [FromQuery] string status,
             [FromQuery] string preference_id,
             [FromQuery] string external_reference,
             CancellationToken cancellationToken)
        {
            var redirectUrl = await _sender.Send(
                new ProcessPaymentReturnCommand(status, preference_id, external_reference, Request.Host.Host), 
                cancellationToken);
                
            return Redirect(redirectUrl);
        }
    }
}
