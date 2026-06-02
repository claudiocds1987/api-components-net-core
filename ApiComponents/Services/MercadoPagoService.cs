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

        public MercadoPagoService(IConfiguration configuration, IOrderRepository orderRepository, AppDbContext context)
        {
            _configuration = configuration;
            _orderRepository = orderRepository;
            _context = context;

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

                        // Mapeamos el objeto para el request que va a viajar a Mercado Pago
                        preferenceItems.Add(new PreferenceItemRequest
                        {
                            Title = product.title, // Tomamos el nombre real de la DB
                            Quantity = item.quantity,
                            UnitPrice = product.price, // Precio real de la DB
                            CurrencyId = "ARS"
                        });
                    }

                    // Asignamos el monto final total calculado a la orden (camelCase)
                    order.totalAmount = totalCalculado;

                    // 3. Guardamos la orden y sus detalles en la DB pasándole el token a SaveChangesAsync
                    await _orderRepository.CreateAsync(order, cancellationToken);
                    await _orderRepository.SaveChangesAsync(cancellationToken);

                    // 4. Configuramos el Request para Mercado Pago usando el ID recién generado como ExternalReference
                    var client = new PreferenceClient();
                    var request = new PreferenceRequest
                    {
                        Items = preferenceItems,

                        BackUrls = new PreferenceBackUrlsRequest
                        {
                            Success = "https://claudiocds1987.github.io/angular-ecommerce-v20/#/payment-result",
                            Failure = "https://claudiocds1987.github.io/angular-ecommerce-v20/#/payment-result",
                            Pending = "https://claudiocds1987.github.io/angular-ecommerce-v20/#/payment-result"
                        },

                        AutoReturn = "approved",
                        BinaryMode = true,

                        // Vinculamos el ID autoincremental de tu base de datos con la transacción de MP
                        ExternalReference = order.id.ToString(),
                        NotificationUrl = $"{_baseUrl}/api/MercadoPago/webhook",

                        PaymentMethods = new PreferencePaymentMethodsRequest
                        {
                            ExcludedPaymentTypes = new List<PreferencePaymentTypeRequest>
                    {
                        new PreferencePaymentTypeRequest { Id = "ticket" }
                    }
                        }
                    };

                    // 5. Enviamos la petición a Mercado Pago pasándole el token de cancelación
                    var preference = await client.CreateAsync(request, null, cancellationToken);
                    finalPreferenceId = preference.Id;

                    // 6. Actualizamos la orden en base de datos con el PreferenceId devuelto por MP (camelCase)
                    order.preferenceId = finalPreferenceId;

                    // Actualizamos el estado invocando a tu repositorio con el token
                    await _orderRepository.UpdateStatusByIdAsync(order.id, "Pending", cancellationToken);
                    await _orderRepository.SaveChangesAsync(cancellationToken);

                    // Confirmamos la transacción de forma atómica en SQL Server pasándole el token
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Si algo sale mal, revertimos todo para evitar órdenes fantasmas o inconsistencias
                    await transaction.RollbackAsync(cancellationToken);

                    Console.WriteLine($"ERROR MERCADO PAGO: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"DETALLE TÉCNICO: {ex.InnerException.Message}");

                    throw new Exception($"Error al generar la preferencia de pago: {ex.Message}", ex);
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