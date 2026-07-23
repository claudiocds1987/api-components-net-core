using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ApiComponents.Application.Features.MercadoPago.Commands.CreatePreference;

public record CreatePreferenceCommand(CartDto Cart) : IRequest<string>;

public class CreatePreferenceCommandHandler : IRequestHandler<CreatePreferenceCommand, string>
{
    private readonly IConfiguration _configuration;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IHostEnvironment _env;

    public CreatePreferenceCommandHandler(
        IConfiguration configuration, 
        IOrderRepository orderRepository, 
        IProductRepository productRepository, 
        IHostEnvironment env)
    {
        _configuration = configuration;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _env = env;
    }

    public async Task<string> Handle(CreatePreferenceCommand request, CancellationToken cancellationToken)
    {
        var token = Environment.GetEnvironmentVariable("MercadoPago__AccessToken");
        var baseUrl = Environment.GetEnvironmentVariable("MercadoPago__BaseUrl");

        if (string.IsNullOrEmpty(token))
        {
            token = _configuration["MercadoPago:AccessToken"];
        }

        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = _configuration["MercadoPago:BaseUrl"];
        }

        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("AccessToken no encontrado en ningún proveedor de configuración.");
        }

        MercadoPagoConfig.AccessToken = token;
        var finalBaseUrl = baseUrl ?? "https://apicomponents.runasp.net";

        // NOTA: Para respetar CQRS y Clean Architecture sin usar AppDbContext en la capa Application,
        // _orderRepository.ExecuteInTransactionAsync debe ser implementado en el repositorio de infraestructura.
        return await _orderRepository.ExecuteInTransactionAsync(async () =>
        {
            var order = new Order
            {
                userId = request.Cart.userId,
                customerEmail = request.Cart.customerEmail,
                customerName = request.Cart.customerName,
                customerPhone = request.Cart.customerPhone,
                shippingAddress = request.Cart.shippingAddress,
                shippingCity = request.Cart.shippingCity,
                shippingZipCode = request.Cart.shippingZipCode,
                status = "Pending",
                createdAt = DateTime.UtcNow,
                orderDetails = new List<OrderDetail>()
            };

            decimal totalAmount = 0;
            var preferenceItems = new List<PreferenceItemRequest>();

            foreach (var item in request.Cart.items)
            {
                var product = await _productRepository.GetProduct(item.productId, cancellationToken);
                if (product == null)
                {
                    throw new Exception($"Producto con ID {item.productId} no encontrado en el catálogo.");
                }

                decimal realPrice = product.price;
                if (product.discountPercentage > 0)
                {
                    realPrice = product.price - (product.price * (product.discountPercentage / 100m));
                }

                var detail = new OrderDetail
                {
                    productId = item.productId,
                    quantity = item.quantity,
                    price = realPrice
                };

                order.orderDetails.Add(detail);
                totalAmount += (realPrice * item.quantity);

                preferenceItems.Add(new PreferenceItemRequest
                {
                    Title = product.title,
                    Quantity = item.quantity,
                    UnitPrice = Math.Round(realPrice, 2),
                    CurrencyId = "ARS"
                });
            }

            order.totalAmount = totalAmount;

            await _orderRepository.CreateAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            var successUrl = "https://localhost:44364/api/MercadoPago/payment-return";
            var failureUrl = "https://localhost:44364/api/MercadoPago/payment-return";
            var pendingUrl = "https://localhost:44364/api/MercadoPago/payment-return";
            var autoReturnBehavior = "approved";

            if (_env.IsProduction())
            {
                string dominioMonster = "https://apicomponents.runasp.net";
                successUrl = $"{dominioMonster}/api/MercadoPago/payment-return";
                failureUrl = $"{dominioMonster}/api/MercadoPago/payment-return";
                pendingUrl = $"{dominioMonster}/api/MercadoPago/payment-return";
                autoReturnBehavior = "approved";
            }

            var client = new PreferenceClient();
            var mpRequest = new PreferenceRequest
            {
                Items = preferenceItems,
                Payer = new PreferencePayerRequest
                {
                    Email = request.Cart.customerEmail?.Trim() ?? "comprador-prueba@test.com"
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = successUrl,
                    Failure = failureUrl,
                    Pending = pendingUrl
                },
                AutoReturn = !string.IsNullOrEmpty(autoReturnBehavior) ? autoReturnBehavior : null,
                BinaryMode = true,
                ExternalReference = order.id.ToString(),
                PaymentMethods = new PreferencePaymentMethodsRequest
                {
                    ExcludedPaymentTypes = new List<PreferencePaymentTypeRequest>
                    {
                        new PreferencePaymentTypeRequest { Id = "ticket" }
                    }
                }
            };

            if (_env.IsProduction() && !string.IsNullOrEmpty(finalBaseUrl) && !finalBaseUrl.Contains("localhost"))
            {
                mpRequest.NotificationUrl = $"{finalBaseUrl.TrimEnd('/')}/api/MercadoPago/webhook";
            }

            var preference = await client.CreateAsync(mpRequest, null, cancellationToken);
            var finalPreferenceId = preference.Id;

            order.preferenceId = finalPreferenceId;

            await _orderRepository.UpdateStatusByIdAsync(order.id, "Pending", cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return finalPreferenceId;
        }, cancellationToken);
    }
}
