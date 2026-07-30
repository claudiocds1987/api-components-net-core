using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Orders.Queries;

public record GetOrderByIdQuery(int Id) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetOrderByIdAsync(request.Id, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {request.Id} not found.");
        }

        return new OrderDto(
            order.id,
            order.preferenceId,
            order.totalAmount,
            order.status,
            order.createdAt,
            order.userId,
            order.customerEmail,
            order.customerName,
            order.customerPhone,
            order.shippingAddress,
            order.shippingCity,
            order.shippingZipCode,
            order.orderDetails.Select(d => new OrderDetailDto(d.id, d.productId, d.quantity, d.price)).ToList()
        );
    }
}
