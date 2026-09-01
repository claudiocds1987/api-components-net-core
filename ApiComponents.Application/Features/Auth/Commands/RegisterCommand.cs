using ApiComponents.Application.DTOs;
using MediatR;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiComponents.Application.Features.Auth.Commands
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

namespace ApiComponents.Application.Features.Auth.Commands
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserResponseDto>
    {
        private readonly IUserRepository _repository;
        private readonly IConfiguration _config;

        public RegisterCommandHandler(IUserRepository repository, IConfiguration config)
        {
            _repository = repository;
            _config = config;
        }

        public async Task<UserResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (await _repository.UserExists(request.username, request.email))
            {
                throw new InvalidOperationException("El usuario o el correo ya estÃ¡n registrados.");
            }

            var user = new User
            {
                username = request.username,
                email = request.email,
                firstName = request.firstName,
                lastName = request.lastName,
                role = request.role,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password)
            };

            await _repository.Create(user);

            return new UserResponseDto
            {
                id = user.id,
                username = user.username,
                email = user.email,
                firstName = user.firstName,
                lastName = user.lastName,
                role = user.role,
                token = GenerateJwtToken(user)
            };
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.Role, user.role)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

