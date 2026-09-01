using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Employees.Commands;

public record AddEmployeeCommand(Employee Employee) : IRequest;

public class AddEmployeeCommandHandler(IEmployeeRepository employeeRepository) : IRequestHandler<AddEmployeeCommand>
{
    public async Task Handle(AddEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (request.Employee.birthDate > DateTime.Now.AddYears(-18))
            throw new ApplicationException("El empleado debe ser mayor de 18 años.");

        await employeeRepository.AddAsync(request.Employee, cancellationToken);
    }
}
