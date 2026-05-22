using ApiComponents.DTOs;
using ApiComponents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiComponents.Persistence.Repositories
{
    public interface IEmployeeRepository
    {
        // Devuelve una lista de empleados paginada, filtrada y ordenada.
        Task<PaginatedList<Employee>> GetPagedEmployeesAsync(EmployeeQueryParams queryParams, CancellationToken cancellationToken = default);

        // Obtener todos los empleados
        Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Employee> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
        // Agregar una lista de empleados (para el endpoint "batch")
        Task AddEmployeeListAsync(List<Employee> employees, CancellationToken cancellationToken = default);
        // Actualizar un empleado
        Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
        // Eliminar un empleado por ID
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Verificar si un empleado existe
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}
