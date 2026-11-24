using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using AutoMapper;
using MediatR;

namespace ApiComponents.Application.Features.Orders.Queries;

/// <summary>
/// Query para listar todos los productos comprados (usuarios registrados e invitados) en el panel de Administrador ("Compras").
/// </summary>
public record GetAdminPurchasedItemsQuery(
    int Page = 1,
    int Size = 25,
    int? UserId = null,
    string? Email = null,
    string? CustomerName = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? Search = null,
    string? Status = null
) : IRequest<PaginatedList<AdminPurchasedItemDto>>;

/// <summary>
/// Handler encargado de consultar y paginar los ítems comprados para la vista de administración.
/// </summary>
public class GetAdminPurchasedItemsQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    : IRequestHandler<GetAdminPurchasedItemsQuery, PaginatedList<AdminPurchasedItemDto>>
{
    public async Task<PaginatedList<AdminPurchasedItemDto>> Handle(GetAdminPurchasedItemsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.Size < 1 ? 25 : request.Size;

        var (items, totalCount) = await orderRepository.GetAdminPurchasedItemsAsync(
            page,
            size,
            request.UserId,
            request.Email,
            request.CustomerName,
            request.DateFrom,
            request.DateTo,
            request.Search,
            request.Status,
            cancellationToken);

        var dtos = mapper.Map<List<AdminPurchasedItemDto>>(items);

        return new PaginatedList<AdminPurchasedItemDto>(dtos, totalCount, page, size);
    }
}
