using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Employees.Queries;

public record GetAllEmployeesQuery() : IRequest<IEnumerable<Employee>>;

public class GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository) : IRequestHandler<GetAllEmployeesQuery, IEnumerable<Employee>>
{
    public async Task<IEnumerable<Employee>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
    {
        return await employeeRepository.GetAllAsync(cancellationToken);
    }
}
