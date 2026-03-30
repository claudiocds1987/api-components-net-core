using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;

namespace ApiComponents.Services;

public class EmployeeService(IEmployeeRepository employeeRepository) : IEmployeeService
{
    public async Task<PaginatedList<Employee>> GetPagedEmployeesAsync(EmployeeQueryParams queryParams)
        => await employeeRepository.GetPagedEmployeesAsync(queryParams);

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        => await employeeRepository.GetAllAsync();

    public async Task<Employee> GetEmployeeByIdAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("El ID del empleado debe ser positivo.");
        return await employeeRepository.GetByIdAsync(id);
    }

    public async Task AddEmployeeAsync(Employee employee)
    {
        if (employee.birthDate > DateTime.Now.AddYears(-18))
            throw new ApplicationException("El empleado debe ser mayor de 18 años.");

        await employeeRepository.AddAsync(employee);
    }

    public async Task UpdateEmployeeAsync(int id, Employee employee)
    {
        if (!await employeeRepository.ExistsAsync(id))
            // CAMBIO AQUÍ: Especificamos el namespace de System para evitar conflicto con GraphQL
            throw new System.Collections.Generic.KeyNotFoundException($"Empleado con ID {id} no encontrado.");

        await employeeRepository.UpdateAsync(employee);
    }

    public async Task DeleteEmployeeAsync(int id) => await employeeRepository.DeleteAsync(id);

    public async Task AddEmployeeListAsync(List<Employee> employees)
    {
        if (employees is not { Count: > 0 }) return;

        if (employees.Count > 500)
            throw new ApplicationException("El tamaño máximo total permitido es 500.");

        var lotes = employees.Chunk(20);

        foreach (var lote in lotes)
        {
            // IDE0305: Uso de spread operator [..] para simplificar
            await employeeRepository.AddEmployeeListAsync([.. lote]);
            await Task.Delay(100);
        }
    }
}