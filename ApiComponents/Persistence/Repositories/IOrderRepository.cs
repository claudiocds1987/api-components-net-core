using ApiComponents.Models;
using System.Threading.Tasks;

namespace ApiComponents.Persistence.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<Order?> GetByPreferenceIdAsync(string preferenceId);
        Task UpdateStatusAsync(string preferenceId, string status);
    }
}