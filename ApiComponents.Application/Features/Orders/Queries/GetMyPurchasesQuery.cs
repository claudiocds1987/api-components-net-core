using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using AutoMapper;
using MediatR;

namespace ApiComponents.Application.Features.Orders.Queries;

/// <summary>
/// Query para obtener el historial de productos comprados y aprobados de un usuario registrado ("Mis Compras").
/// </summary>
public record GetMyPurchasesQuery(
    int? UserId,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    int? CategoryId = null,
    string? Search = null
) : IRequest<List<MyPurchaseItemDto>>;

/// <summary>
/// Handler encargado de validar el usuario y consultar sus compras aprobadas ordenadas de más reciente a más antigua.
/// </summary>
public class GetMyPurchasesQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    : IRequestHandler<GetMyPurchasesQuery, List<MyPurchaseItemDto>>
{
    public async Task<List<MyPurchaseItemDto>> Handle(GetMyPurchasesQuery request, CancellationToken cancellationToken)
    {
        if (!request.UserId.HasValue || request.UserId.Value <= 0)
        {
            throw new ArgumentException("El parámetro userId es obligatorio y debe ser un identificador válido.");
        }

        var items = await orderRepository.GetUserApprovedPurchasesAsync(
            request.UserId.Value,
            request.DateFrom,
            request.DateTo,
            request.CategoryId,
            request.Search,
            cancellationToken);

        return mapper.Map<List<MyPurchaseItemDto>>(items);
    }
}
