using ApiComponents.DTOs;
using ApiComponents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiComponents.Persistence.Repositories
{
    public interface IEmployeeRepository
    {
        // Devuelve una lista de empleados paginada, filtrada y ordenada.
        Task<PaginatedList<Employee>> GetPagedEmployeesAsync(EmployeeQueryParams queryParams);

        // Obtener todos los empleados
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee> GetByIdAsync(int id);
        Task AddAsync(Employee employee);
        // Agregar una lista de empleados (para el endpoint "batch")
        Task AddEmployeeListAsync(List<Employee> employees);
        // Actualizar un empleado
        Task UpdateAsync(Employee employee);
        // Eliminar un empleado por ID
        Task DeleteAsync(int id);

        // Verificar si un empleado existe
        Task<bool> ExistsAsync(int id);
    }
}
