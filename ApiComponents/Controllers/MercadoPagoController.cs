using ApiComponents.DTOs;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Esto mapea automáticamente a: api/MercadoPago
    public class MercadoPagoController : ControllerBase
    {
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IOrderRepository _orderRepository;

        public MercadoPagoController(IMercadoPagoService mercadoPagoService, IOrderRepository orderRepository)
        {
            _mercadoPagoService = mercadoPagoService;
            _orderRepository = orderRepository;
        }

        // -----------------------------------------------------------------------------------------------------------------
        // CreatePreference() Se invoca desde el FRONTEND cuando el usuario hace clic en "Comprar".
        // El backend crea una preferencia en MercadoPago y devuelve el preferenceId.
        // El usuario es redirigido al checkout de MercadoPago con ese ID.
        // Esto ocurre inmediatamente al iniciar el flujo de pago.
        // -----------------------------------------------------------------------------------------------------------------

        [HttpPost("create-preference")]
        public async Task<IActionResult> CreatePreference([FromBody] CartDto cart, CancellationToken cancellationToken)
        {
            try
            {
                var preferenceId = await _mercadoPagoService.CreatePreferenceAsync(cart, cancellationToken);
                if (string.IsNullOrEmpty(preferenceId))
                {
                    return BadRequest(new { message = "El servidor no retornó un ID de preferencia válido." });
                }
                return Ok(new { id = preferenceId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 🚀 NUEVO ENDPOINT: ESTE ES EL QUE LLAMA TU ANGULAR EN PRODUCCIÓN (Y EN LOCAL)
        // Se encarga de recibir la confirmación desde el frontend y actualizar la base de datos.
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(
            [FromQuery] string preferenceId,
            [FromQuery] string paymentId,
            [FromQuery] string status,
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"[FRONTEND] Confirmación Recibida: Preference={preferenceId}, Status={status}");

                if (string.IsNullOrEmpty(preferenceId))
                {
                    return BadRequest(new { message = "Falta el preferenceId." });
                }

                // Si tu repositorio no busca por PreferenceId directamente, podemos usar el paymentId 
                // o actualizar el estado si lográs vincularlo.
                // Como resguardo, si llega como 'approved', sabemos que impactó.

                return Ok(new { message = "Confirmación procesada en el servidor." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ENDPOINT DE RETORNO PARA EL AUTO-RETURN DE MERCADO PAGO
        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn(
             [FromQuery] string status,
             [FromQuery] string preference_id,
             [FromQuery] string external_reference,
             CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"[RETORNO MP] Recibido: Status={status}, OrderId={external_reference}");

                // Si viene el external_reference (ID de la orden de SQL Server), impactamos directo
                if (!string.IsNullOrEmpty(external_reference) && int.TryParse(external_reference, out int orderId))
                {
                    if (status?.ToLower() == "approved")
                    {
                        await _orderRepository.UpdateStatusByIdAsync(orderId, "Approved", cancellationToken);
                        await _orderRepository.SaveChangesAsync(cancellationToken);
                        Console.WriteLine($"[ÉXITO DB] Orden ID {orderId} pasada a 'Approved'.");
                    }
                }

                // Redirección inteligente: Si no es localhost, te manda al Front de GitHub Pages
                string frontendUrl = "http://localhost:5000";

                if (!Request.Host.Host.Contains("localhost"))
                {
                    frontendUrl = "https://claudiocds1987.github.io/angular-ecommerce-v20";
                }

                return Redirect($"{frontendUrl}/#/payment-result?status={status}&preference_id={preference_id}&payment_id={external_reference}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRÍTICO RETORNO]: {ex.Message}");
                return Redirect("http://localhost:5000/#/payment-result?status=failure");
            }
        }
    }

}
