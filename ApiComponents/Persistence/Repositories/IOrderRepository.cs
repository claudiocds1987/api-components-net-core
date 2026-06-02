using ApiComponents.Domain;

namespace ApiComponents.Persistence.Repositories
{
    public interface IOrderRepository
    {
        Task CreateAsync(Order order, CancellationToken cancellationToken = default);
        Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

        // Buscamos por ID numérico de base de datos (ExternalReference)
        Task UpdateStatusByIdAsync(int id, string status, CancellationToken cancellationToken = default);

        // Opcional: Mantenemos la búsqueda por PreferenceId para el refuerzo del Frontend
        Task UpdateStatusByPreferenceIdAsync(string preferenceId, string status, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}