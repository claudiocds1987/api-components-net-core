using ApiComponents.Application.DTOs;
using MediatR;

namespace ApiComponents.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<UserResponseDto>
    {
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
    }
}
