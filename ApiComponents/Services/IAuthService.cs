using ApiComponents.Application.DTOs;

namespace ApiComponents.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto?> Login(string username, string password);
        Task<UserResponseDto?> Register(RegisterDto registerDto);
        // Para el endpoint /me
        Task<UserResponseDto?> GetUserByUsernameAsync(string username);
    }
}
