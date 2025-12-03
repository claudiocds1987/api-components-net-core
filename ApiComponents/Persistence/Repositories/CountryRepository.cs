using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Necesario para AnyAsync

namespace ApiComponents.Persistence.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _context;

        public CountryRepository(AppDbContext context)
        {
            _context = context;
        }

        // --- READ ---
        public async Task<IEnumerable<Country>> GetAllAsync()
        {
            return await _context.Country.ToListAsync();
        }

        public async Task<Country> GetByIdAsync(int id)
        {
            return await _context.Country.FindAsync(id);
        }

        // --- CREATE ---
        public async Task AddAsync(Country country)
        {
            // Agrega la entidad al contexto
            _context.Country.Add(country);

            // Guarda los cambios en la base de datos
            await _context.SaveChangesAsync();
        }

        // --- UPDATE ---
        public async Task UpdateAsync(Country country)
        {
            // Indica a EF Core que el estado de la entidad ha sido modificado
            _context.Entry(country).State = EntityState.Modified;

            // Guarda los cambios
            await _context.SaveChangesAsync();
        }

        // --- DELETE ---
        public async Task DeleteAsync(int id)
        {
            // 1. Encuentra la entidad. FindAsync es ideal para buscar por PK.
            var country = await _context.Country.FindAsync(id);

            if (country != null)
            {
                // 2. Marca la entidad para eliminación
                _context.Country.Remove(country);

                // 3. Guarda los cambios (ejecuta el DELETE)
                await _context.SaveChangesAsync();
            }
        }

        // --- EXISTS CHECK ---
        public async Task<bool> ExistsAsync(int id)
        {
            // Usa AnyAsync para una verificación de existencia eficiente
            return await _context.Country.AnyAsync(c => c.id == id);
        }
        // -- CHECKEA SI LA DESCRIPCIÓN DEL PAIS YA EXISTE 
        public async Task<bool> ExistsByDescriptionAsync(string description)
        {
            // Utilizamos AnyAsync para verificar si existe algún país con esa descripción.
            return await _context.Country.AnyAsync(c => c.description == description);
        }
    }
}
