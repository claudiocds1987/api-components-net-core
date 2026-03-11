using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task<User?> GetByUsername(string username) =>
            await context.Users.FirstOrDefaultAsync(u => u.username == username);

        public async Task<User> Create(User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExists(string username, string email) =>
            await context.Users.AnyAsync(u => u.username == username || u.email == email);


    }
}
