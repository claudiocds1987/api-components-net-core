using ApiComponents.DTOs;

namespace ApiComponents.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto?> Login(string username, string password);
        Task<UserResponseDto?> Register(RegisterDto registerDto);
    }
}
