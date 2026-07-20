using ApiComponents.Application.DTOs;
using MediatR;

namespace ApiComponents.Application.Features.Auth.Queries.GetUserByUsername
{
    public class GetUserByUsernameQuery : IRequest<UserResponseDto>
    {
        public string username { get; set; } = string.Empty;
    }
}
