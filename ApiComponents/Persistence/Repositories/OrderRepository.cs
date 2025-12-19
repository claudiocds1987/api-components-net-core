using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ApiComponents.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Order order)
        {
            await _context.Set<Order>().AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetByPreferenceIdAsync(string preferenceId)
        {
            return await _context.Set<Order>().FirstOrDefaultAsync(o => o.PreferenceId == preferenceId);
        }

        public async Task UpdateStatusAsync(string preferenceId, string status)
        {
            var order = await GetByPreferenceIdAsync(preferenceId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
        }
    }
}