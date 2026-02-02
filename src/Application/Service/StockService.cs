using Application.Dtos.Stock;
using Application.Exceptions;
using Application.Interfaces;
using Application.Service.Interfaces;
using Domain.Entities;

namespace Application.Service
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<StockEntryResponse>> GetAllStockEntries()
        {
            var entries = await _unitOfWork.StockEntries.GetAll();
            return entries.Select(MapToStockEntryResponse);
        }

        public async Task<IEnumerable<StockEntryResponse>> GetStockEntriesByProductId(Guid productId)
        {
            var entries = await _unitOfWork.StockEntries.GetByProductId(productId);
            return entries.Select(MapToStockEntryResponse);
        }

        public async Task<StockEntryResponse> AddStockToProduct(StockEntryRequest request, Guid userId)
        {
            var product = await _unitOfWork.Products.GetById(request.ProductId);
            if (product == null)
                throw new NotFoundException("Produto", request.ProductId);

            var stockEntry = new StockEntry
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                InvoiceNumber = request.InvoiceNumber,
                EntryDate = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            await _unitOfWork.StockEntries.Add(stockEntry);

            product.StockQuantity += request.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);

            await _unitOfWork.SaveChangesAsync();

            var savedEntry = await _unitOfWork.StockEntries.GetById(stockEntry.Id);
            return MapToStockEntryResponse(savedEntry!);
        }

        private static StockEntryResponse MapToStockEntryResponse(StockEntry entry)
        {
            return new StockEntryResponse
            {
                Id = entry.Id,
                ProductId = entry.ProductId,
                ProductName = entry.Product?.Name ?? string.Empty,
                Quantity = entry.Quantity,
                InvoiceNumber = entry.InvoiceNumber,
                EntryDate = entry.EntryDate,
                CreatedByUserName = entry.CreatedBy?.Name ?? string.Empty
            };
        }
    }
}
