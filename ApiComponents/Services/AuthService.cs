using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiComponents.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository repository, IConfiguration config)
        {
            _repository = repository;
            _config = config;
        }

        public async Task<UserResponseDto?> Login(string username, string password)
        {
            var user = await _repository.GetByUsername(username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.passwordHash)) return null;

            return CreateResponse(user);
        }

        public async Task<UserResponseDto?> Register(RegisterDto dto)
        {
            if (await _repository.UserExists(dto.username, dto.email)) return null;

            var user = new User
            {
                username = dto.username,
                email = dto.email,
                firstName = dto.firstName,
                lastName = dto.lastName,
                role = dto.role,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.password)
            };

            await _repository.Create(user);
            return CreateResponse(user);
        }

        private UserResponseDto CreateResponse(User user)
        {
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
