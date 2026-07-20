using ApiComponents.Application.DTOs;
using MediatR;

namespace ApiComponents.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<UserResponseDto>
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
}
