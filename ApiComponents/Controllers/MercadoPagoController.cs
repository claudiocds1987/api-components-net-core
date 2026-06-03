//using ApiComponents.DTOs;
//using ApiComponents.Persistence.Repositories;
//using ApiComponents.Services;
//using Microsoft.AspNetCore.Mvc;

//[ApiController]
//[Route("api/[controller]")]
//public class MercadoPagoController : ControllerBase
//{
//    private readonly IMercadoPagoService _mpService;
//    private readonly IOrderRepository _orderRepository;

//    public MercadoPagoController(IMercadoPagoService mpService, IOrderRepository orderRepository)
//    {
//        _mpService = mpService;
//        _orderRepository = orderRepository;
//    }

//    // -----------------------------------------------------------------------------------------------------------------
//    // CreatePreference() Se invoca desde el FRONTEND cuando el usuario hace clic en "Comprar".
//    // El backend crea una preferencia en MercadoPago y devuelve el preferenceId.
//    // El usuario es redirigido al checkout de MercadoPago con ese ID.
//    // Esto ocurre inmediatamente al iniciar el flujo de pago.
//    // -----------------------------------------------------------------------------------------------------------------
//    [HttpPost("create-preference")]
//    public async Task<IActionResult> CreatePreference([FromBody] CartDto cart)
//    {
//        try
//        {
//            var preferenceId = await _mpService.CreatePreferenceAsync(cart);
//            return Ok(new { id = preferenceId });
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, new { error = "Error en el servidor backend", detalle = ex.Message });
//        }
//    }

//    // -----------------------------------------------------------------------------------------------------------------
//    // [HttpPost("webhook")] WEBHOOK PROFESIONAL: Recibe notificaciones asincrónicas de MP
//    // NO lo llama el frontend. MercadoPagoWebhook() Lo dispara AUTOMÁTICAMENTE MercadoPago
//    // después de que el usuario completa el pago en su plataforma.
//    // MercadoPago envía un POST a esta URL con información del pago.
//    // El backend consulta el estado real en MercadoPago y actualiza la orden.
//    // Esto ocurre asincrónicamente, cuando MercadoPago termina de procesar el pago.
//    // -----------------------------------------------------------------------------------------------------------------
//    [HttpPost("webhook")]
//    public async Task<IActionResult> MercadoPagoWebhook([FromQuery] string topic, [FromQuery] string id)
//    {
//        if (topic == "payment" || string.IsNullOrEmpty(topic))
//        {
//            try
//            {
//                var client = new MercadoPago.Client.Payment.PaymentClient();
//                var payment = await client.GetAsync(long.Parse(id));

//                if (payment.Status == "approved")
//                {
//                    // ¡AQUÍ ESTÁ EL CAMBIO CLAVE!: 
//                    // Mercado Pago nos devuelve en 'ExternalReference' el ID exacto que le mandamos de nuestra DB (ej: "1")
//                    if (int.TryParse(payment.ExternalReference, out int orderId))
//                    {
//                        // Actualizamos usando el método por ID numérico
//                        await _orderRepository.UpdateStatusByIdAsync(orderId, "Approved");
//                        await _orderRepository.SaveChangesAsync();

//                        Console.WriteLine($"[WEBHOOK] Orden {orderId} aprobada con éxito en la Base de Datos.");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                // Logueamos el error pero siempre devolvemos Ok() para que MP no reintente infinitamente
//                Console.WriteLine($"Error procesando Webhook: {ex.Message}");
//            }
//        }

//        // Confirmamos recepción a Mercado Pago
//        return Ok();
//    }

//    // -----------------------------------------------------------------------------------------------------------------
//    // confirm-payment (CONFIRMACIÓN SEGURA DESDE EL FRONTEND)
//    // Endpoint opcional para verificación manual.
//    // Se puede invocar desde el FRONTEND (por ejemplo, al volver del checkout)
//    // o desde un proceso interno para validar el estado del pago.
//    // El backend consulta el estado en MercadoPago y actualiza la orden si está aprobado.
//    // Esto ocurre solo si vos decidís llamarlo explícitamente como refuerzo de seguridad.
//    // -----------------------------------------------------------------------------------------------------------------
//    [HttpPost("confirm-payment")]
//    public async Task<IActionResult> ConfirmPayment(
//    [FromBody] MercadoPagoConfirmationDto confirmation,
//    [FromServices] IWebHostEnvironment env)
//    {
//        // Traza de depuración para desarrollo: nos permite auditar los datos entrantes en la consola
//        Console.WriteLine($"[ConfirmPayment] Recibido - PreferenceId: {confirmation.PreferenceId}, PaymentId: {confirmation.PaymentId}, Status: {confirmation.Status}");

//        // Validación preventiva de parámetros de entrada
//        if (string.IsNullOrEmpty(confirmation.PreferenceId))
//        {
//            return BadRequest("El campo PreferenceId es obligatorio para actualizar la orden.");
//        }

//        // 1. COMPORTAMIENTO EN PRODUCCIÓN (MonsterASP)
//        if (env.IsProduction())
//        {
//            try
//            {
//                var realStatus = await _mpService.GetPaymentStatusAsync(confirmation.PaymentId);

//                if (realStatus == "approved")
//                {
//                    await _orderRepository.UpdateStatusByPreferenceIdAsync(confirmation.PreferenceId, "Approved");
//                    await _orderRepository.SaveChangesAsync();
//                    return Ok(new { message = "Pago verificado y aprobado con éxito en producción." });
//                }

//                if (realStatus == "in_process" || realStatus == "pending")
//                {
//                    await _orderRepository.UpdateStatusByPreferenceIdAsync(confirmation.PreferenceId, "Pending");
//                    await _orderRepository.SaveChangesAsync();
//                    return Ok(new { message = "Orden registrada en estado pendiente." });
//                }

//                return BadRequest("El pago no pudo ser verificado de forma segura.");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error en la verificación de seguridad: {ex.Message}");
//            }
//        }

//        // 2. COMPORTAMIENTO EN DESARROLLO (Tu localhost)
//        // Forzamos la actualización directa usando el PreferenceId que vemos en tu tabla de SQL Server
//        try
//        {
//            await _orderRepository.UpdateStatusByPreferenceIdAsync(confirmation.PreferenceId, "Approved");
//            await _orderRepository.SaveChangesAsync();

//            Console.WriteLine($"[ConfirmPayment] Orden con PreferenceId {confirmation.PreferenceId} actualizada a 'Approved' con éxito.");
//            return Ok(new { message = "Simulación de pago aprobada con éxito localmente (Entorno de Desarrollo)." });
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, $"Error al actualizar la base de datos local: {ex.Message}");
//        }
//    }
//    //[HttpPost("confirm-payment")]
//    //public async Task<IActionResult> ConfirmPayment(
//    //[FromBody] MercadoPagoConfirmationDto confirmation,
//    //[FromServices] IWebHostEnvironment env) // IWebHostEnvironment es nativa de net core, es para detectar si estamos en Producción o Desarrollo
//    //{
//    //    // 1. SI ESTAMOS EN PRODUCCIÓN (MonsterASP), ACTIVAMOS LA SEGURIDAD MÁXIMA
//    //    if (env.IsProduction())
//    //    {
//    //        try
//    //        {
//    //            // Validamos el estado REAL del pago consultando directamente a la API de Mercado Pago
//    //            var realStatus = await _mpService.GetPaymentStatusAsync(confirmation.PaymentId);

//    //            if (realStatus == "approved")
//    //            {
//    //                await _orderRepository.UpdateStatusByPreferenceIdAsync(confirmation.PreferenceId, "Approved");
//    //                await _orderRepository.SaveChangesAsync();

//    //                return Ok(new { message = "Pago verificado y aprobado con éxito en producción." });
//    //            }

//    //            return BadRequest("El pago no pudo ser verificado de forma segura.");
//    //        }
//    //        catch (Exception ex)
//    //        {
//    //            return StatusCode(500, $"Error al verificar el pago con Mercado Pago: {ex.Message}");
//    //        }
//    //    }

//    //    // 2. SI ESTAMOS EN DESARROLLO (en localhost), FORZAMOS LA CONFIRMACIÓN RÁPIDA
//    //    // Esto evita que tus pruebas locales se traben si Mercado Pago Sandbox tarda en procesar
//    //    await _orderRepository.UpdateStatusByPreferenceIdAsync(confirmation.PreferenceId, "Approved");
//    //    await _orderRepository.SaveChangesAsync();

//    //    return Ok(new { message = "Simulación de pago aprobada con éxito localmente (Entorno de Desarrollo)." });
//    //}



//}

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

        // ENDPOINT DE RETORNO CORREGIDO, SEGURO Y SIN MÉTODOS INEXISTENTES
        [HttpGet("payment-return")] // URL: https://localhost:44364/api/MercadoPago/payment-return
        public async Task<IActionResult> PaymentReturn(
            [FromQuery] string status,
            [FromQuery] string preference_id,
            [FromQuery] string external_reference,
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"[RETORNO MP] Recibido: Status={status}, Preference={preference_id}, OrderId={external_reference}");

                // Si viene el external_reference (que es el ID autoincremental de tu orden de SQL Server)
                if (!string.IsNullOrEmpty(external_reference) && int.TryParse(external_reference, out int orderId))
                {
                    if (status?.ToLower() == "approved")
                    {
                        // Usamos tus métodos nativos del repositorio que ya sabemos que funcionan
                        await _orderRepository.UpdateStatusByIdAsync(orderId, "Approved", cancellationToken);
                        await _orderRepository.SaveChangesAsync(cancellationToken);
                        Console.WriteLine($"[ÉXITO] Orden ID {orderId} actualizada a 'Approved' en SQL Server.");
                    }
                }
                else
                {
                    Console.WriteLine("[ADVERTENCIA] No se pudo procesar el estado en la DB porque no llegó un external_reference válido.");
                }

                // Redirección directa al frontend de Angular local con los parámetros limpios
                return Redirect($"http://localhost:5000/#/payment-result?status={status}&preference_id={preference_id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRÍTICO EN CONTROLADOR]: {ex.Message}");
                // Si algo falla, mandamos al usuario al front igual para que no vea una pantalla rota
                return Redirect($"http://localhost:5000/#/payment-result?status=failure");
            }
        }
    }
}