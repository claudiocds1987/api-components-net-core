using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Employees.Queries;

public record GetEmployeeByIdQuery(int Id) : IRequest<Employee>;

public class GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository) : IRequestHandler<GetEmployeeByIdQuery, Employee>
{
    public async Task<Employee> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            throw new ArgumentException("El ID del empleado debe ser positivo.");

        return await employeeRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}
