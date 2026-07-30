using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Repositories
{
    public interface IOrderRepository
    {
        Task CreateAsync(Order order, CancellationToken cancellationToken = default);
        Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

        // Buscamos por ID numérico de base de datos (ExternalReference)
        Task UpdateStatusByIdAsync(int id, string status, CancellationToken cancellationToken = default);

        // Opcional: Mantenemos la búsqueda por PreferenceId para el refuerzo del Frontend
        Task UpdateStatusByPreferenceIdAsync(string preferenceId, string status, CancellationToken cancellationToken = default);

        Task<List<Order>> GetExpiredPendingOrdersAsync(DateTime expirationTime, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<(List<Order> Items, int TotalCount)> GetPagedOrdersAsync(
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortOrder,
            string? customerEmail,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken = default);

        Task<List<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}