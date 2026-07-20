using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Auth.Queries.GetUserByUsername
{
    public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, UserResponseDto>
    {
        private readonly IUserRepository _repository;

        public GetUserByUsernameQueryHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserResponseDto> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByUsername(request.username);

            if (user == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            return new UserResponseDto
            {
                id = user.id,
                username = user.username,
                email = user.email,
                firstName = user.firstName,
                lastName = user.lastName,
                role = user.role,
                token = "" // En el endpoint /me, el token ya lo tiene el cliente, lo dejamos vacío
            };
        }
    }
}
