using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Orders.Commands;

public record CancelExpiredOrdersCommand(int ExpirationMinutes = 15) : IRequest<Unit>;

public class CancelExpiredOrdersCommandHandler : IRequestHandler<CancelExpiredOrdersCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public CancelExpiredOrdersCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    /// <summary>
    /// Este Handle maneja la cancelación automática de órdenes que quedaron pendientes y superaron el tiempo límite de pago.
    /// Resuelve el problema de las compras abandonadas: permite reservarle el stock al usuario al iniciar el pago para que nadie lo "robe", 
    /// Ejemplo: si un usuario inicia el pago de un producto y lo deja en el carrito, ese stock queda reservado para él y no puede ser comprado por otro usuario.
    /// pero si el usuario cierra la pestaña y nunca paga, este proceso libera el stock reservado para que vuelva a estar disponible 
    /// y no quede descontado para siempre.
    /// </summary>
    public async Task<Unit> Handle(CancelExpiredOrdersCommand request, CancellationToken cancellationToken)
    {
        // 1. Calcula el límite de tiempo para que el usuario complete la orden no mas de 15 minutos
        var expirationTime = DateTime.UtcNow.AddMinutes(-request.ExpirationMinutes);
        // 2. Obtiene todas las órdenes pendientes que han expirado en la base de datos todas
        var expiredOrders = await _orderRepository.GetExpiredPendingOrdersAsync(expirationTime, cancellationToken);
        // 3. Recorre una por una cada orden vencida encontrada
        foreach (var order in expiredOrders)
        {
            // 4. Inicia una transacción para asegurar que si algo falla, no queden datos a medias
            await _orderRepository.ExecuteInTransactionAsync(async () =>
            {
                // 5. Recorre cada producto (detalle) que formaba parte de esa orden vencida
                foreach (var detail in order.orderDetails)
                {
                    // 6. Devuelve al inventario general la cantidad de stock que estaba reservada
                    // Ej: el usuario había pedido 2 unidades del producto X, pero no completó la compra, entonces esas 2 unidades se devuelven al stock disponible
                    await _productRepository.RestoreProductStock(detail.productId, detail.quantity, cancellationToken);
                }

                // 7. Actualiza el estado de la orden en la base de datos a "Cancelled" (cancelada)
                await _orderRepository.UpdateStatusByIdAsync(order.id, "Cancelled", cancellationToken);
                // 8. Guarda los cambios en la base de datos con la orden en estado "Cancelled"
                await _orderRepository.SaveChangesAsync(cancellationToken);
                // 9. Devuelve el valor unitario requerido por MediatR para finalizar la transacción con éxito
                return Unit.Value;
            }, cancellationToken);
        }
        // 10. Devuelve el resultado final del Handler indicando que todo el proceso terminó correctamente
        return Unit.Value;
    }
}
