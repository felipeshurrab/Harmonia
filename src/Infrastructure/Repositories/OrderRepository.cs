using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly HarmoniaDbContext _dbContext;

        public OrderRepository(HarmoniaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Seller)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetBySellerId(Guid sellerId)
        {
            return await _dbContext.Orders
                .Where(o => o.SellerId == sellerId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetById(Guid id)
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Seller)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> Add(Order order)
        {
            await _dbContext.Orders.AddAsync(order);
            return order;
        }
    }
}
