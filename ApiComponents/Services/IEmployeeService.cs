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
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<PaginatedList<Employee>> GetPagedEmployeesAsync(EmployeeQueryParams queryParams);
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task AddEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(int id, Employee employee);
        Task DeleteEmployeeAsync(int id);
        Task AddEmployeeListAsync(List<Employee> employees);
    }
}