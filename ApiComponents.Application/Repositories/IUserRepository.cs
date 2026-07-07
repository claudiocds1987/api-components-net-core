using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsername(string username);
        Task<User> Create(User user);
        Task<bool> UserExists(string username, string email);
    }
}
