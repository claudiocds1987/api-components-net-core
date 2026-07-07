using ApiComponents.Domain.Models;

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
