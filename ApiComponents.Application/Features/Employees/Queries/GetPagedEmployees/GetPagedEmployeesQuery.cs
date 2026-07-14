using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Employees.Queries.GetPagedEmployees;

public record GetPagedEmployeesQuery(EmployeeQueryParams QueryParams) : IRequest<PaginatedList<Employee>>;

public class GetPagedEmployeesQueryHandler(IEmployeeRepository employeeRepository) : IRequestHandler<GetPagedEmployeesQuery, PaginatedList<Employee>>
{
    public async Task<PaginatedList<Employee>> Handle(GetPagedEmployeesQuery request, CancellationToken cancellationToken)
    {
        return await employeeRepository.GetPagedEmployeesAsync(request.QueryParams, cancellationToken);
    }
}
