using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Orders.Queries;

public record GetOrdersMetricsQuery() : IRequest<DashboardDto>;

public record DashboardDto(
    DashboardMetrics Metrics,
    List<SalesEvolutionDto> SalesEvolution,
    List<OrderStatusDistributionDto> OrderStatuses,
    List<TopProductDto> TopProducts,
    List<RecentOrderDto> RecentOrders);

public record DashboardMetrics(
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue);

public record SalesEvolutionDto(string Date, decimal Revenue);

public record OrderStatusDistributionDto(string Status, int Count);

public record TopProductDto(string ProductName, int Quantity);

public record RecentOrderDto(int Id, string CustomerName, string Destination, DateTime Date, decimal TotalAmount, string Status);

public class GetOrdersMetricsQueryHandler : IRequestHandler<GetOrdersMetricsQuery, DashboardDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public GetOrdersMetricsQueryHandler(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<DashboardDto> Handle(GetOrdersMetricsQuery request, CancellationToken cancellationToken)
    {
        var allOrders = await _orderRepository.GetAllOrdersAsync(cancellationToken);

        var totalOrders = allOrders.Count;
        var totalRevenue = allOrders.Sum(o => o.totalAmount);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var metrics = new DashboardMetrics(totalRevenue, totalOrders, Math.Round(averageOrderValue, 2));

        var salesEvolution = allOrders
            .GroupBy(o => o.createdAt.Date)
            .Select(g => new SalesEvolutionDto(g.Key.ToString("yyyy-MM-dd"), g.Sum(o => o.totalAmount)))
            .OrderBy(s => s.Date)
            .TakeLast(30) // Mostrar últimos 30 días si hay muchos
            .ToList();

        var orderStatuses = allOrders
            .GroupBy(o => o.status)
            .Select(g => new OrderStatusDistributionDto(g.Key, g.Count())) // Cuenta la cantidad de pedidos por estado
            .ToList();

        var topProductsRaw = allOrders
            .SelectMany(o => o.orderDetails) // Selecciona todos los detalles de los pedidos
            .GroupBy(d => d.productId) // Agrupa por ID de producto
            .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(d => d.quantity) }) // Suma la cantidad total vendida por producto
            .OrderByDescending(x => x.TotalQuantity)
            .Take(5) // Muestra los 5 productos más vendidos
            .ToList();

        var topProducts = new List<TopProductDto>();
        foreach (var tp in topProductsRaw)
        {
            var product = await _productRepository.GetProduct(tp.ProductId, cancellationToken);
            string productName = product != null ? product.title : $"Producto #{tp.ProductId}";
            topProducts.Add(new TopProductDto(productName, tp.TotalQuantity));
        }

        var recentOrders = allOrders
            .OrderByDescending(o => o.createdAt)
            .Take(7)
            .Select(o => new RecentOrderDto(
                o.id,
                o.customerName,
                o.shippingCity,
                o.createdAt,
                o.totalAmount,
                o.status
            ))
            .ToList();

        return new DashboardDto(metrics, salesEvolution, orderStatuses, topProducts, recentOrders);
    }
}
