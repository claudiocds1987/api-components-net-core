using ApiComponents.Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ApiComponents.Application.Features.MercadoPago.Commands.ProcessPaymentReturn;

public record ProcessPaymentReturnCommand(string Status, string PreferenceId, string ExternalReference, string Host) : IRequest<string>;

public class ProcessPaymentReturnCommandHandler : IRequestHandler<ProcessPaymentReturnCommand, string>
{
    private readonly IOrderRepository _orderRepository;

    public ProcessPaymentReturnCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<string> Handle(ProcessPaymentReturnCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[RETORNO MP] Recibido: Status={request.Status}, OrderId={request.ExternalReference}");

        if (!string.IsNullOrEmpty(request.ExternalReference) && int.TryParse(request.ExternalReference, out int orderId))
        {
            if (request.Status?.ToLower() == "approved")
            {
                await _orderRepository.UpdateStatusByIdAsync(orderId, "Approved", cancellationToken);
                await _orderRepository.SaveChangesAsync(cancellationToken);
                Console.WriteLine($"[ÉXITO DB] Orden ID {orderId} pasada a 'Approved'.");
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
