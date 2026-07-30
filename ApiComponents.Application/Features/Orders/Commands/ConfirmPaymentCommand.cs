using MediatR;

namespace ApiComponents.Application.Features.Orders.Commands;

public record ConfirmPaymentCommand(string PreferenceId, string PaymentId, string Status) : IRequest<string>;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, string>
{
    public Task<string> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        // Opcional: Aquí podrías agregar lógica para verificar en base de datos si se requiere
        Console.WriteLine($"[FRONTEND] Confirmación Recibida: Preference={request.PreferenceId}, PaymentId={request.PaymentId}, Status={request.Status}");
        return Task.FromResult("Confirmación procesada en el servidor.");
    }
}
