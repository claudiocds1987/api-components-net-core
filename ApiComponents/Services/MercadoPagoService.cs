using ApiComponents.Domain;
using ApiComponents.DTOs;
using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Services
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _context; // Inyectado para validar y congelar precios reales del catálogo
        private readonly string _baseUrl;
        private readonly IWebHostEnvironment _env;

        public MercadoPagoService(IConfiguration configuration, IOrderRepository orderRepository, AppDbContext context, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _orderRepository = orderRepository;
            _context = context;
            _env = env;


            // 1. Intenta leer de Variable de Entorno (MonsterASP)
            var token = Environment.GetEnvironmentVariable("MercadoPago__AccessToken");
            var baseUrl = Environment.GetEnvironmentVariable("MercadoPago__BaseUrl");

            // 2. Si es NULL (estás en local), lee del appsettings.json
            if (string.IsNullOrEmpty(token))
            {
                token = _configuration["MercadoPago:AccessToken"];
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = _configuration["MercadoPago:BaseUrl"];
            }

            // 3. Validación final
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("AccessToken no encontrado en ningún proveedor de configuración.");
            }

            MercadoPagoConfig.AccessToken = token;
            _baseUrl = baseUrl ?? "https://apicomponents.runasp.net";
        }

        public async Task<string> CreatePreferenceAsync(CartDto cart, CancellationToken cancellationToken = default)
        {
            // 1. Pasamos el cancellationToken a la estrategia de ejecución
            var strategy = _context.Database.CreateExecutionStrategy();
            string finalPreferenceId = string.Empty;

            await strategy.ExecuteAsync(async () =>
            {
                // 2. Pasamos el cancellationToken al iniciar la transacción
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // 1. Instanciamos el objeto Order con las variables en camelCase y los datos del comprador/envío
                    var order = new Order
                    {
                        userId = cart.userId, // Puede ser null si compra como invitado
                        customerEmail = cart.customerEmail,
                        customerName = cart.customerName,
                        customerPhone = cart.customerPhone,
                        shippingAddress = cart.shippingAddress,
                        shippingCity = cart.shippingCity,
                        shippingZipCode = cart.shippingZipCode,
                        status = "Pending",
                        createdAt = DateTime.UtcNow
                    };

                    decimal totalCalculado = 0;
                    var preferenceItems = new List<PreferenceItemRequest>();

                    // 2. Validamos cada ítem contra la Base de Datos para congelar el precio real actual
                    foreach (var item in cart.items)
                    {
                        // Buscamos el producto en el catálogo pasándole el token
                        var product = await _context.Products.FindAsync(new object[] { item.productId }, cancellationToken);
                        if (product == null)
                        {
                            throw new Exception($"Producto con ID {item.productId} no encontrado en el catálogo.");
                        }

                        // Creamos la línea de detalle con el precio histórico/congelado (camelCase)
                        var detail = new OrderDetail
                        {
                            productId = item.productId,
                            quantity = item.quantity,
                            price = product.price // Usamos el 'price' real de tu base de datos
                        };

                        order.orderDetails.Add(detail);

                        // Calculamos el subtotal acumulado basándonos en la DB por seguridad
                        totalCalculado += (product.price * item.quantity);

                        // Mapeamos el objeto para el request sanitizando el UnitPrice
                        preferenceItems.Add(new PreferenceItemRequest
                        {
                            Title = product.title, // Tomamos el nombre real de la DB
                            Quantity = item.quantity,
                            // Forzamos un redondeo a 2 decimales para evitar floats extraños
                            UnitPrice = Math.Round((decimal)product.price, 2),
                            CurrencyId = "ARS"
                        });
                    }

                    // Asignamos el monto final total calculado a la orden (camelCase)
                    order.totalAmount = totalCalculado;

                    // 3. Guardamos la orden y sus detalles en la DB pasándole el token a SaveChangesAsync
                    await _orderRepository.CreateAsync(order, cancellationToken);
                    await _orderRepository.SaveChangesAsync(cancellationToken);

                    // 4. CONFIGURACIÓN DE URLS - DIRECTA Y FIJA PARA TU PC LOCAL
                    // Usamos el puerto HTTPS de tu API para que Mercado Pago acepte el AutoReturn en local
                    var successUrl = "https://localhost:44364/api/MercadoPago/payment-return";
                    var failureUrl = "https://localhost:44364/api/MercadoPago/payment-return";
                    var pendingUrl = "https://localhost:44364/api/MercadoPago/payment-return";

                    var autoReturnBehavior = "approved";

                    // Si el backend detecta de forma estricta que está montado en MonsterASP, pisa con HTTPS de GitHub
                    if (_env.IsProduction())
                    {
                        successUrl = "https://claudiocds1987.github.io/angular-ecommerce-v20/#/payment-result";
                        failureUrl = "https://claudiocds1987.github.io/angular-ecommerce-v20/#/payment-result";
                        pendingUrl = "https://claudiocds1987.github.io/angular-ecommerce-v20/#/payment-result";

                        // Activamos el retorno automático únicamente en producción porque las URLs usan HTTPS seguro
                        autoReturnBehavior = "approved";
                    }

                    // 5. Configuramos el Request para Mercado Pago usando el ID recién generado como ExternalReference
                    var client = new PreferenceClient();
                    var request = new PreferenceRequest
                    {
                        Items = preferenceItems,

                        Payer = new PreferencePayerRequest
                        {
                            Email = cart.customerEmail?.Trim() ?? "comprador-prueba@test.com"
                        },

                        BackUrls = new PreferenceBackUrlsRequest
                        {
                            Success = successUrl,
                            Failure = failureUrl,
                            Pending = pendingUrl
                        },

                        // CONFIGURACIÓN INTELIGENTE: Si está en local va vacío para evitar el error 400. En producción usa "approved"
                        AutoReturn = !string.IsNullOrEmpty(autoReturnBehavior) ? autoReturnBehavior : null,
                        BinaryMode = true,

                        // Vinculamos el ID autoincremental de tu base de datos con la transacción de MP
                        ExternalReference = order.id.ToString(),

                        // MANTENEMOS TU CONFIGURACIÓN INTACTA DE MEDIOS DE PAGO EXCLUYENDO SÓLO TICKET
                        PaymentMethods = new PreferencePaymentMethodsRequest
                        {
                            ExcludedPaymentTypes = new List<PreferencePaymentTypeRequest>
                     {
                         new PreferencePaymentTypeRequest { Id = "ticket" }
                     }
                        }
                    };

                    // Bloque de notificación para el webhook en producción
                    if (_env.IsProduction() && !string.IsNullOrEmpty(_baseUrl) && !_baseUrl.Contains("localhost"))
                    {
                        // webhook recibe notificaciones asincrónicas de MP
                        // NO lo llama el frontend. Lo dispara AUTOMÁTICAMENTE MercadoPago
                        // después de que el usuario completa el pago en su plataforma.
                        // MercadoPago envía un POST a esta URL con información del pago.
                        // El backend consulta el estado real en MercadoPago y actualiza la orden.
                        // Sirve como respaldo de seguridad si el usuario cierra la pestaña antes de la redirección.
                        request.NotificationUrl = $"{_baseUrl.TrimEnd('/')}/api/MercadoPago/webhook";
                    }

                    // 6. Enviamos la petición a Mercado Pago pasándole el token de cancelación
                    var preference = await client.CreateAsync(request, null, cancellationToken);
                    finalPreferenceId = preference.Id;

                    // 7. Actualizamos la orden en base de datos con el PreferenceId devuelto por MP (camelCase)
                    order.preferenceId = finalPreferenceId;

                    // Actualizamos el estado de la orden creada
                    await _orderRepository.UpdateStatusByIdAsync(order.id, "Pending", cancellationToken);
                    await _orderRepository.SaveChangesAsync(cancellationToken);

                    // Confirmamos la transacción de forma atómica en SQL Server pasándole el token
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Si algo sale mal, revertimos todo para evitar órdenes fantasmas o inconsistencias
                    await transaction.RollbackAsync(cancellationToken);

                    // Imprimimos el error real en la consola de Visual Studio para auditarlo
                    Console.WriteLine($"[CRÍTICO] ERROR INTERNO EN ENDPOINT: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[CRÍTICO] DETALLE INTERNO: {ex.InnerException.Message}");
                    }

                    throw;
                }
            });

            return finalPreferenceId;

        }

        public async Task<string> GetPaymentStatusAsync(string paymentId)
        {
            var client = new PaymentClient();
            var payment = await client.GetAsync(long.Parse(paymentId));
            return payment.Status;
        }
    }
}