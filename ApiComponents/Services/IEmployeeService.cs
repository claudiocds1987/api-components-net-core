using ApiComponents.Models;
using ApiComponents.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiComponents.Services
{
    // Esta interfaz es lo que el controlador consumirá.
    public interface IEmployeeService
    {
        // Métodos CRUD para la lógica de negocio
        // El servicio maneja la paginación, delegando la consulta al repositorio.
        Task<IEnumerable<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
        Task<PaginatedList<Employee>> GetPagedEmployeesAsync(EmployeeQueryParams queryParams, CancellationToken cancellationToken = default);
        Task<Employee> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddEmployeeAsync(Employee employee, CancellationToken cancellationToken = default);
        Task UpdateEmployeeAsync(int id, Employee employee, CancellationToken cancellationToken = default);
        Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
        Task AddEmployeeListAsync(List<Employee> employees, CancellationToken cancellationToken = default);
    }
}