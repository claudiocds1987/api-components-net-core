using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Orders.Queries;

public record GetAllOrdersQuery(
    int PageNumber = 1,
    int PageSize = 25,
    string SortColumn = "id",
    string SortOrder = "desc",
    string? UserEmail = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<PagedOrdersResult>;

public record PagedOrdersResult(List<OrderDto> Items, int TotalCount, int PageNumber, int PageSize);

public record OrderDto(
    int Id,
    string? PreferenceId,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    int? UserId,
    string CustomerEmail,
    string CustomerName,
    string? CustomerPhone,
    string ShippingAddress,
    string ShippingCity,
    string ShippingZipCode,
    List<OrderDetailDto> Details);

public record OrderDetailDto(
    int Id,
    int ProductId,
    int Quantity,
    decimal Price);

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PagedOrdersResult>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedOrdersResult> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _orderRepository.GetPagedOrdersAsync(
            request.PageNumber,
            request.PageSize,
            request.SortColumn,
            request.SortOrder,
            request.UserEmail,
            request.DateFrom,
            request.DateTo,
            cancellationToken);

        var dtos = items.Select(o => new OrderDto(
            o.id,
            o.preferenceId,
            o.totalAmount,
            o.status,
            o.createdAt,
            o.userId,
            o.customerEmail,
            o.customerName,
            o.customerPhone,
            o.shippingAddress,
            o.shippingCity,
            o.shippingZipCode,
            o.orderDetails.Select(d => new OrderDetailDto(d.id, d.productId, d.quantity, d.price)).ToList()
        )).ToList();

        return new PagedOrdersResult(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
