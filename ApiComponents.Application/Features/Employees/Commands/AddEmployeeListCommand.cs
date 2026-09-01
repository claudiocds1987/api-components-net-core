using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Employees.Commands;

public record AddEmployeeListCommand(List<Employee> Employees) : IRequest;

public class AddEmployeeListCommandHandler(IEmployeeRepository employeeRepository) : IRequestHandler<AddEmployeeListCommand>
{
    public async Task Handle(AddEmployeeListCommand request, CancellationToken cancellationToken)
    {
        if (request.Employees is not { Count: > 0 }) return;

        if (request.Employees.Count > 500)
            throw new ApplicationException("El tamaño máximo total permitido es 500.");

        var lotes = request.Employees.Chunk(20);

        foreach (var lote in lotes)
        {
            await employeeRepository.AddEmployeeListAsync([.. lote], cancellationToken);
            await Task.Delay(100, cancellationToken);
        }
    }
}
