using ApiComponents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiComponents.Persistence.Repositories
{
    public interface ICountryRepository
    {
        Task<IEnumerable<Country>> GetAllAsync();
        Task<Country> GetByIdAsync(int id);
        Task AddAsync(Country country);
        Task UpdateAsync(Country country);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByDescriptionAsync(string description);
    }
}