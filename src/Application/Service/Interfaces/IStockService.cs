using Application.Dtos.Stock;

namespace Application.Service.Interfaces
{
    public interface IStockService
    {
        Task<IEnumerable<StockEntryResponse>> GetAllStockEntries();
        Task<IEnumerable<StockEntryResponse>> GetStockEntriesByProductId(Guid productId);
        Task<StockEntryResponse> AddStockToProduct(StockEntryRequest request, Guid userId);
    }
}
