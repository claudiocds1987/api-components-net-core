using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

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
            // Simplemente delegamos la acción al Repositorio. 
            // Podríamos añadir lógica de caché o filtros globales aquí.
            return await _countryRepository.GetAllAsync();
        }

        // --- READ: Obtener por ID ---
        public async Task<Country> GetCountryByIdAsync(int id)
        {
            // 1. Validación de ID de entrada
            if (id <= 0)
            {
                throw new ArgumentException("El ID del país debe ser positivo.");
            }

            // 2. Llama al Repositorio
            var country = await _countryRepository.GetByIdAsync(id);

            // 3. Lógica de Negocio: Si no existe, lanza una excepción para que el Controller devuelva 404
            if (country == null)
            {
                throw new KeyNotFoundException($"País con ID {id} no encontrado.");
            }

            return country;
        }

        // --- CREATE: Agregar País ---
        public async Task AddCountryAsync(Country country)
        {
            // Lógica de Negocio: 1. Validación de datos del Dominio
            if (string.IsNullOrWhiteSpace(country.description))
            {
                throw new ArgumentException("La descripción del país es obligatoria.");
            }

            // Lógica de Negocio: 2. Validar unicidad (Patrón de Verificación en el Servicio)
            if (await _countryRepository.ExistsByDescriptionAsync(country.description))
            {
                // Lanzamos una excepción de negocio clara. 
                // El Controlador deberá capturarla y devolver un HTTP 409 Conflict.
                throw new InvalidOperationException($"El país '{country.description}' ya se encuentra registrado.");
            }

            await _countryRepository.AddAsync(country);
        }

        // --- UPDATE: Actualizar País ---
        public async Task UpdateCountryAsync(int id, Country country)
        {
            // 1. Verificación de existencia antes de actualizar
            if (!await _countryRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"País con ID {id} no encontrado para actualizar.");
            }

            // 2. Lógica de Negocio: Asegurarse de que el objeto tenga el ID correcto
            country.id = id;

            // 3. Llama al Repositorio para actualizar
            await _countryRepository.UpdateAsync(country);
        }

        // --- DELETE: Eliminar País ---
        public async Task DeleteCountryAsync(int id)
        {
            // 1. Verificación de existencia
            if (!await _countryRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"País con ID {id} no encontrado para eliminar.");
            }

            // 2. Lógica de Negocio (Ejemplo: No permitir eliminar si hay empleados asociados)
            // Esto requeriría una inyección de IEmployeeRepository y una consulta. 
            // Por simplicidad, asumiremos que las restricciones de clave foránea de la DB lo manejan.

            // 3. Llama al Repositorio para eliminar
            await _countryRepository.DeleteAsync(id);
        }
    }
}