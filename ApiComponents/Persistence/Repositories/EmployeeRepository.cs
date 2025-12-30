using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiComponents.Persistence.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            // FindAsync es el método más eficiente para buscar una entidad por su clave primaria.
            // Primero verifica el caché de seguimiento de cambios de EF Core y luego va a la base de datos.
            return await _context.Employees.FindAsync(id);
        }

        public async Task<PaginatedList<Employee>> GetPagedEmployeesAsync(EmployeeQueryParams queryParams)
        {
            IQueryable<Employee> query = _context.Employees.AsQueryable();

            // 1. Aplicar FILTROS (WHERE)

            // --- Filtro por ID (exacto) ---
            if (queryParams.Id.HasValue)
            {
                query = query.Where(e => e.id == queryParams.Id.Value);
            }

            // --- Búsqueda por Nombre (contiene) ---
            if (!string.IsNullOrWhiteSpace(queryParams.Name))
            {
                // NOTA: Usamos Contains para buscar coincidencias parciales.
                query = query.Where(e => e.name.Contains(queryParams.Name));
            }

            // FILTRO AGREGADO: Búsqueda por Apellido (contiene) 🌟
            if (!string.IsNullOrWhiteSpace(queryParams.Surname))
            {
                query = query.Where(e => e.surname.Contains(queryParams.Surname));
            }

            // FILTRO AGREGADO: Filtrar por ID de País (exacto) 🌟
            if (queryParams.CountryId.HasValue)
            {
                query = query.Where(e => e.countryId == queryParams.CountryId.Value);
            }

            // FILTRO AGREGADO: Filtrar por Fecha de Nacimiento (rango o exacto, si EmployeeQueryParams lo soporta) 🌟
            // NOTA: Asumimos que quieres filtrar por una fecha de nacimiento exacta si se proporciona, 
            // o podrías definir un rango (StartDate/EndDate) en el DTO para búsquedas más flexibles.
            if (queryParams.BirthDate.HasValue)
            {
                query = query.Where(e => e.birthDate.Date == queryParams.BirthDate.Value.Date);
            }

            // FILTRO AGREGADO: Filtrar por ID de Posición (exacto) 🌟
            if (queryParams.PositionId.HasValue)
            {
                query = query.Where(e => e.positionId == queryParams.PositionId.Value);
            }

            // FILTRO AGREGADO: Filtrar por Estado Activo (booleano exacto) 🌟
            if (queryParams.Active.HasValue)
            {
                query = query.Where(e => e.active == queryParams.Active.Value);
            }

            // FILTRO AGREGADO: Filtrar por ID de Género (exacto) 🌟
            if (queryParams.GenderId.HasValue)
            {
                query = query.Where(e => e.genderId == queryParams.GenderId.Value);
            }


            // 2. Obtener el TOTAL de registros antes de aplicar Skip/Take
            var totalCount = await query.CountAsync();

            // 3. Aplicar ORDENAMIENTO (ORDER BY)
            string sortColumn = queryParams.SortColumn?.ToLower() ?? "id";
            string sortOrder = queryParams.SortOrder?.ToLower() ?? "asc";

            switch (sortColumn)
            {
                case "name":
                    query = sortOrder == "desc" ? query.OrderByDescending(e => e.name) : query.OrderBy(e => e.name);
                    break;
                // ORDENAMIENTO AGREGADO: Por Apellido
                case "surname":
                    query = sortOrder == "desc" ? query.OrderByDescending(e => e.surname) : query.OrderBy(e => e.surname);
                    break;
                case "id":
                default:
                    query = sortOrder == "desc" ? query.OrderByDescending(e => e.id) : query.OrderBy(e => e.id);
                    break;
            }

            // 4. Aplicar PAGINACIÓN (SKIP y TAKE) y ejecutar la consulta
            var items = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            // 5. Devolver el objeto PaginatedList
            return new PaginatedList<Employee>(
                items,
                totalCount,
                queryParams.PageNumber,
                queryParams.PageSize
            );
        }

        // Implementación de otros métodos CRUD
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task AddEmployeeListAsync(List<Employee> employees)
        {
            try
            {
                // 1. Agregamos el rango de 20 empleados al contexto
                await _context.Employees.AddRangeAsync(employees);

                // 2. Guardamos en la base de datos
                await _context.SaveChangesAsync();

                // 3. LIMPIEZA DE MEMORIA (Crucial para el plan gratuito)
                // Esto le dice a EF Core que deje de rastrear los registros que ya guardó.
                // Evita que el contexto crezca infinitamente y consuma RAM del servidor.
                _context.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                // Si hay un error, lo lanzamos para que el Service pueda capturarlo
                throw new Exception($"Error en repositorio al procesar lote: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Employee employee)
        {
            // Indica a EF Core que el estado de la entidad ha sido modificado
            // Esto le dice a EF Core que debe generar una sentencia UPDATE.
            _context.Entry(employee).State = EntityState.Modified;

            // Guarda los cambios en la base de datos
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // 1. Encuentra la entidad que se va a eliminar
            var employee = await _context.Employees.FindAsync(id);

            if (employee != null)
            {
                // 2. Marca la entidad para eliminación
                _context.Employees.Remove(employee);

                // 3. Guarda los cambios (ejecuta la sentencia DELETE)
                await _context.SaveChangesAsync();
            }
            // NOTA: Si FindAsync devuelve null, simplemente la función termina. 
            // La validación de NotFound() ya la maneja el Service o Controller.
        }

        public async Task<bool> ExistsAsync(int id)
        {
            // Usa AnyAsync para verificar de manera eficiente si existe un registro con ese ID.
            // Esto se traduce en una consulta SELECT 1 o COUNT(*) en SQL, que es muy rápido.
            return await _context.Employees.AnyAsync(e => e.id == id);
        }

    }
}
