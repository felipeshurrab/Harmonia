using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.DbContexts;
using Infrastructure.Repositories;

namespace Infrastructure.UoW
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; private set; }
        public IProductRepository Products { get; private set; }
        public IStockEntryRepository StockEntries { get; private set; }
        public IOrderRepository Orders { get; private set; }

        private readonly HarmoniaDbContext _dbContext;

        public UnitOfWork(HarmoniaDbContext dbContext)
        {
            _dbContext = dbContext;
            Users = new UserRepository(_dbContext);
            Products = new ProductRepository(_dbContext);
            StockEntries = new StockEntryRepository(_dbContext);
            Orders = new OrderRepository(_dbContext);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dbContext.DisposeAsync();
        }
    }
}
