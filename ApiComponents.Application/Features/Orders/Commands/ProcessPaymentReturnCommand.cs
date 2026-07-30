using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Orders.Commands;

public record ProcessPaymentReturnCommand(string Status, string PreferenceId, string ExternalReference, string Host) : IRequest<string>;

public class ProcessPaymentReturnCommandHandler : IRequestHandler<ProcessPaymentReturnCommand, string>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public ProcessPaymentReturnCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<string> Handle(ProcessPaymentReturnCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.ExternalReference) && int.TryParse(request.ExternalReference, out int orderId))
        {
            var status = request.Status?.ToLower();
            if (status == "approved")
            {
                await _orderRepository.UpdateStatusByIdAsync(orderId, "Approved", cancellationToken);
                await _orderRepository.SaveChangesAsync(cancellationToken);
            }
            else if (status == "rejected" || status == "cancelled" || status == "null")
            {
                await _orderRepository.ExecuteInTransactionAsync(async () =>
                {
                    var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
                    if (order != null && order.status == "Pending")
                    {
                        foreach (var detail in order.orderDetails)
                        {
                            await _productRepository.RestoreProductStock(detail.productId, detail.quantity, cancellationToken);
                        }
                        await _orderRepository.UpdateStatusByIdAsync(orderId, "Rejected", cancellationToken);
                        await _orderRepository.SaveChangesAsync(cancellationToken);
                    }
                    return Unit.Value;
                }, cancellationToken);
            }
        }

        string frontendUrl = "http://localhost:5000";
        if (!request.Host.Contains("localhost"))
        {
            frontendUrl = "https://claudiocds1987.github.io/angular-ecommerce-v20";
        }

        return $"{frontendUrl}/#/payment-result?status={request.Status}&preference_id={request.PreferenceId}&payment_id={request.ExternalReference}";
    }
}
