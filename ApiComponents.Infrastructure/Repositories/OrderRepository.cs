using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            _context.Orders.Update(order);
            await Task.CompletedTask;
        }

        // Busca por el ID numérico (Ideal para el Webhook via ExternalReference)
        public async Task UpdateStatusByIdAsync(int id, string status, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.id == id, cancellationToken);
            if (order != null)
            {
                order.status = status;
            }
        }

        // Busca por el string largo de Mercado Pago (Ideal para la confirmación del Frontend)
        public async Task UpdateStatusByPreferenceIdAsync(string preferenceId, string status, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.preferenceId == preferenceId, cancellationToken);
            if (order != null)
            {
                order.status = status;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await action();
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Order>> GetExpiredPendingOrdersAsync(DateTime expirationTime, CancellationToken cancellationToken = default)
        {
            // Usamos Include(o => o.orderDetails) para traer la información de cuánto stock debemos devolver
            return await _context.Orders
                .Include(o => o.orderDetails)
                .Where(o => o.status == "Pending" && o.createdAt < expirationTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.orderDetails)
                .FirstOrDefaultAsync(o => o.id == id, cancellationToken);
        }

        public async Task<(List<Order> Items, int TotalCount)> GetPagedOrdersAsync(
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortOrder,
            string? customerEmail,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(customerEmail))
            {
                query = query.Where(o => o.customerEmail.Contains(customerEmail));
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(o => o.createdAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                var adjustedDateTo = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.createdAt <= adjustedDateTo);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = sortColumn.ToLower() switch
            {
                "id" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.id) : query.OrderBy(o => o.id),
                "orderdate" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.createdAt) : query.OrderBy(o => o.createdAt),
                "createdat" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.createdAt) : query.OrderBy(o => o.createdAt),
                "userid" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.userId) : query.OrderBy(o => o.userId),
                "shippingaddress" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.shippingAddress) : query.OrderBy(o => o.shippingAddress),
                "shippingcity" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.shippingCity) : query.OrderBy(o => o.shippingCity),
                "totalamount" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.totalAmount) : query.OrderBy(o => o.totalAmount),
                "status" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.status) : query.OrderBy(o => o.status),
                _ => sortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.id) : query.OrderBy(o => o.id)
            };

            var items = await query
                .Include(o => o.orderDetails)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<List<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.orderDetails)
                .ToListAsync(cancellationToken);
        }
    }
}