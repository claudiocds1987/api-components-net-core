using ApiComponents.Domain.Models;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;

        public CountryService(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        // --- READ: Obtener todos ---
        public async Task<IEnumerable<Country>> GetAllCountriesAsync()
        {
            return await _countryRepository.GetAllAsync();
        }

        // --- READ: Obtener por ID ---
        public async Task<Country> GetCountryByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID del país debe ser positivo.");
            }

            var country = await _countryRepository.GetByIdAsync(id);

            if (country == null)
            {
                // CAMBIO AQUÍ: Nombre completo para evitar ambigüedad con GreenDonut
                throw new System.Collections.Generic.KeyNotFoundException($"País con ID {id} no encontrado.");
            }

            return country;
        }

        // --- CREATE: Agregar País ---
        public async Task AddCountryAsync(Country country)
        {
            if (string.IsNullOrWhiteSpace(country.description))
            {
                throw new ArgumentException("La descripción del país es obligatoria.");
            }

            if (await _countryRepository.ExistsByDescriptionAsync(country.description))
            {
                throw new InvalidOperationException($"El país '{country.description}' ya se encuentra registrado.");
            }

            await _countryRepository.AddAsync(country);
        }

        // --- UPDATE: Actualizar País ---
        public async Task UpdateCountryAsync(int id, Country country)
        {
            if (!await _countryRepository.ExistsAsync(id))
            {
                // CAMBIO AQUÍ: Nombre completo
                throw new System.Collections.Generic.KeyNotFoundException($"País con ID {id} no encontrado para actualizar.");
            }

            country.id = id;

            await _countryRepository.UpdateAsync(country);
        }

        // --- DELETE: Eliminar País ---
        public async Task DeleteCountryAsync(int id)
        {
            if (!await _countryRepository.ExistsAsync(id))
            {
                // CAMBIO AQUÍ: Nombre completo
                throw new System.Collections.Generic.KeyNotFoundException($"País con ID {id} no encontrado para eliminar.");
            }

            await _countryRepository.DeleteAsync(id);
        }
    }
}