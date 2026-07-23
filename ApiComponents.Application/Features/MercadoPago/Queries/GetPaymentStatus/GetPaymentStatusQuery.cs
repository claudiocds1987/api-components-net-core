using MediatR;
using MercadoPago.Client.Payment;

namespace ApiComponents.Application.Features.MercadoPago.Queries.GetPaymentStatus;

public record GetPaymentStatusQuery(string PaymentId) : IRequest<string>;

public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, string>
{
    public async Task<string> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        var client = new PaymentClient();
        var payment = await client.GetAsync(long.Parse(request.PaymentId), cancellationToken: cancellationToken);
        return payment.Status;
    }
}
