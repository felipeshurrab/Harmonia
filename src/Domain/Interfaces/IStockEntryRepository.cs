using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IStockEntryRepository
    {
        Task<IEnumerable<StockEntry>> GetAll();
        Task<IEnumerable<StockEntry>> GetByProductId(Guid productId);
        Task<StockEntry?> GetById(Guid id);
        Task<StockEntry> Add(StockEntry stockEntry);
    }
}
