using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Services;

public class EmployeeService(IEmployeeRepository employeeRepository) : IEmployeeService
{
    public async Task<PaginatedList<Employee>> GetPagedEmployeesAsync(EmployeeQueryParams queryParams, CancellationToken cancellationToken = default)
        => await employeeRepository.GetPagedEmployeesAsync(queryParams, cancellationToken);

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
        => await employee_repository_GetAllAsync_wrapper(cancellationToken);

    private async Task<IEnumerable<Employee>> employee_repository_GetAllAsync_wrapper(CancellationToken cancellationToken)
    {
        return await employeeRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Employee> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ArgumentException("El ID del empleado debe ser positivo.");
        return await employeeRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task AddEmployeeAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        if (employee.birthDate > DateTime.Now.AddYears(-18))
            throw new ApplicationException("El empleado debe ser mayor de 18 años.");

        await employeeRepository.AddAsync(employee, cancellationToken);
    }

    public async Task UpdateEmployeeAsync(int id, Employee employee, CancellationToken cancellationToken = default)
    {
        if (!await employeeRepository.ExistsAsync(id, cancellationToken))
            // CAMBIO AQUÍ: Especificamos el namespace de System para evitar conflicto con GraphQL
            throw new System.Collections.Generic.KeyNotFoundException($"Empleado con ID {id} no encontrado.");

        await employeeRepository.UpdateAsync(employee, cancellationToken);
    }

    public async Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default) => await employeeRepository.DeleteAsync(id, cancellationToken);

    public async Task AddEmployeeListAsync(List<Employee> employees, CancellationToken cancellationToken = default)
    {
        if (employees is not { Count: > 0 }) return;

        if (employees.Count > 500)
            throw new ApplicationException("El tamaño máximo total permitido es 500.");

        var lotes = employees.Chunk(20);

        foreach (var lote in lotes)
        {
            await employeeRepository.AddEmployeeListAsync([.. lote], cancellationToken);
            await Task.Delay(100, cancellationToken);
        }
    }
}