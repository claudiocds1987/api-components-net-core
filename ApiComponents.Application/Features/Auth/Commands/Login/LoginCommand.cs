using ApiComponents.Application.DTOs;
using MediatR;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiComponents.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<UserResponseDto>
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
}

namespace ApiComponents.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, UserResponseDto>
    {
        private readonly IUserRepository _repository;
        private readonly IConfiguration _config;

        public LoginCommandHandler(IUserRepository repository, IConfiguration config)
        {
            _repository = repository;
            _config = config;
        }

        public async Task<UserResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByUsername(request.username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.password, user.passwordHash))
            {
                throw new UnauthorizedAccessException("Usuario o contraseÃ±a incorrectos.");
            }

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

