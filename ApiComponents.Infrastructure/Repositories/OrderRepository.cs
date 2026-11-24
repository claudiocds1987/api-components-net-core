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

        public async Task<List<OrderDetail>> GetUserApprovedPurchasesAsync(
            int userId,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? categoryId,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var query = BuildPurchasedItemsBaseQuery()
                .Where(d => d.order.userId == userId && d.order.status == "Approved");

            query = ApplyCommonPurchaseFilters(query, dateFrom, dateTo, search);

            if (categoryId.HasValue)
            {
                query = query.Where(d => d.product != null && d.product.categoryId == categoryId.Value);
            }

            return await query
                .OrderByDescending(d => d.order.createdAt)
                .ThenByDescending(d => d.id)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<OrderDetail> Items, int TotalCount)> GetAdminPurchasedItemsAsync(
            int pageNumber,
            int pageSize,
            int? userId,
            string? email,
            string? customerName,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? search,
            string? status,
            CancellationToken cancellationToken = default)
        {
            var query = BuildPurchasedItemsBaseQuery();

            if (userId.HasValue)
            {
                query = query.Where(d => d.order.userId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var trimmedEmail = email.Trim();
                query = query.Where(d => d.order.customerEmail.Contains(trimmedEmail));
            }

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                var trimmedName = customerName.Trim();
                query = query.Where(d => d.order.customerName.Contains(trimmedName));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var trimmedStatus = status.Trim();
                query = query.Where(d => d.order.status == trimmedStatus);
            }

            query = ApplyCommonPurchaseFilters(query, dateFrom, dateTo, search);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(d => d.order.createdAt)
                .ThenByDescending(d => d.id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        private IQueryable<OrderDetail> BuildPurchasedItemsBaseQuery()
        {
            return _context.OrderDetails
                .AsNoTracking()
                .Include(d => d.order)
                .Include(d => d.product!)
                    .ThenInclude(p => p.category)
                .Include(d => d.product!)
                    .ThenInclude(p => p.images)
                .AsSplitQuery();
        }

        private static IQueryable<OrderDetail> ApplyCommonPurchaseFilters(
            IQueryable<OrderDetail> query,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? search)
        {
            if (dateFrom.HasValue)
            {
                query = query.Where(d => d.order.createdAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                var adjustedDateTo = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(d => d.order.createdAt <= adjustedDateTo);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var trimmedSearch = search.Trim();
                query = query.Where(d => d.product != null && d.product.title.Contains(trimmedSearch));
            }

            return query;
        }
    }
}