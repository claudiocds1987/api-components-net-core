using ApiComponents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiComponents.Services
{
    public interface ICountryService
    {
        Task<IEnumerable<Country>> GetAllCountriesAsync();
        Task<Country> GetCountryByIdAsync(int id);
        Task AddCountryAsync(Country country);
        Task UpdateCountryAsync(int id, Country country);
        Task DeleteCountryAsync(int id);
    }
}
