using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(int Id) : IRequest;

public class DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository) : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        await employeeRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
