using ApiComponents.DTOs;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

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

    // -----------------------------------------------------------------------------------------------------------------
    // CreatePreference() Se invoca desde el FRONTEND cuando el usuario hace clic en "Comprar".
    // El backend crea una preferencia en MercadoPago y devuelve el preferenceId.
    // El usuario es redirigido al checkout de MercadoPago con ese ID.
    // Esto ocurre inmediatamente al iniciar el flujo de pago.
    // -----------------------------------------------------------------------------------------------------------------
    [HttpPost("create-preference")]
    public async Task<IActionResult> CreatePreference([FromBody] CartDto cart)
    {
        try
        {
            var preferenceId = await _mpService.CreatePreferenceAsync(cart);
            return Ok(new { id = preferenceId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error en el servidor backend", detalle = ex.Message });
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // [HttpPost("webhook")] WEBHOOK PROFESIONAL: Recibe notificaciones asincrónicas de MP
    // NO lo llama el frontend. MercadoPagoWebhook() Lo dispara AUTOMÁTICAMENTE MercadoPago
    // después de que el usuario completa el pago en su plataforma.
    // MercadoPago envía un POST a esta URL con información del pago.
    // El backend consulta el estado real en MercadoPago y actualiza la orden.
    // Esto ocurre asincrónicamente, cuando MercadoPago termina de procesar el pago.
    // -----------------------------------------------------------------------------------------------------------------
    [HttpPost("webhook")]
    public async Task<IActionResult> MercadoPagoWebhook([FromQuery] string topic, [FromQuery] string id)
    {
        if (topic == "payment" || string.IsNullOrEmpty(topic))
        {
            try
            {
                var client = new MercadoPago.Client.Payment.PaymentClient();
                var payment = await client.GetAsync(long.Parse(id));

                if (payment.Status == "approved")
                {
                    // ¡AQUÍ ESTÁ EL CAMBIO CLAVE!: 
                    // Mercado Pago nos devuelve en 'ExternalReference' el ID exacto que le mandamos de nuestra DB (ej: "1")
                    if (int.TryParse(payment.ExternalReference, out int orderId))
                    {
                        // Actualizamos usando el método por ID numérico
                        await _orderRepository.UpdateStatusByIdAsync(orderId, "Approved");
                        await _orderRepository.SaveChangesAsync();

                        Console.WriteLine($"[WEBHOOK] Orden {orderId} aprobada con éxito en la Base de Datos.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Logueamos el error pero siempre devolvemos Ok() para que MP no reintente infinitamente
                Console.WriteLine($"Error procesando Webhook: {ex.Message}");
            }
        }

        // Confirmamos recepción a Mercado Pago
        return Ok();
    }

    // -----------------------------------------------------------------------------------------------------------------
    // confirm-payment (CONFIRMACIÓN SEGURA DESDE EL FRONTEND)
    // Endpoint opcional para verificación manual.
    // Se puede invocar desde el FRONTEND (por ejemplo, al volver del checkout)
    // o desde un proceso interno para validar el estado del pago.
    // El backend consulta el estado en MercadoPago y actualiza la orden si está aprobado.
    // Esto ocurre solo si vos decidís llamarlo explícitamente como refuerzo de seguridad.
    // -----------------------------------------------------------------------------------------------------------------
    [HttpPost("confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(
    [FromBody] MercadoPagoConfirmationDto confirmation,
    [FromServices] IWebHostEnvironment env) // IWebHostEnvironment es nativa de net core, es para detectar si estamos en Producción o Desarrollo
    {
        // 1. SI ESTAMOS EN PRODUCCIÓN (MonsterASP), ACTIVAMOS LA SEGURIDAD MÁXIMA
        if (env.IsProduction())
        {
            try
            {
                // Validamos el estado REAL del pago consultando directamente a la API de Mercado Pago
                var realStatus = await _mpService.GetPaymentStatusAsync(confirmation.PaymentId);

                if (realStatus == "approved")
                {
                    await _orderRepository.UpdateStatusByPreferenceIdAsync(confirmation.PreferenceId, "Approved");
                    await _orderRepository.SaveChangesAsync();

                    return Ok(new { message = "Pago verificado y aprobado con éxito en producción." });
                }

                return BadRequest("El pago no pudo ser verificado de forma segura.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al verificar el pago con Mercado Pago: {ex.Message}");
            }
        }

        // 2. SI ESTAMOS EN DESARROLLO (en localhost), FORZAMOS LA CONFIRMACIÓN RÁPIDA
        // Esto evita que tus pruebas locales se traben si Mercado Pago Sandbox tarda en procesar
        await _orderRepository.UpdateStatusByPreferenceIdAsync(confirmation.PreferenceId, "Approved");
        await _orderRepository.SaveChangesAsync();

        return Ok(new { message = "Simulación de pago aprobada con éxito localmente (Entorno de Desarrollo)." });
    }



}