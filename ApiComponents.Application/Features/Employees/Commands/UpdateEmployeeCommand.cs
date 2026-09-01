using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Employees.Commands;

public record UpdateEmployeeCommand(int Id, Employee Employee) : IRequest;

public class UpdateEmployeeCommandHandler(IEmployeeRepository employeeRepository) : IRequestHandler<UpdateEmployeeCommand>
{
    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (!await employeeRepository.ExistsAsync(request.Id, cancellationToken))
            throw new System.Collections.Generic.KeyNotFoundException($"Empleado con ID {request.Id} no encontrado.");

        await employeeRepository.UpdateAsync(request.Employee, cancellationToken);
    }
}
