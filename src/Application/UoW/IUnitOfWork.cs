using Domain.Interfaces;

namespace Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IProductRepository Products { get; }
        IStockEntryRepository StockEntries { get; }
        IOrderRepository Orders { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}