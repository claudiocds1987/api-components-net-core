using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsername(string username);
        Task<User> Create(User user);
        Task<bool> UserExists(string username, string email);
    }
}
