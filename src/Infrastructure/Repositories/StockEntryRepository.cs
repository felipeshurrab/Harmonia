using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StockEntryRepository : IStockEntryRepository
    {
        private readonly HarmoniaDbContext _dbContext;

        public StockEntryRepository(HarmoniaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<StockEntry>> GetAll()
        {
            return await _dbContext.StockEntries
                .Include(s => s.Product)
                .Include(s => s.CreatedBy)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockEntry>> GetByProductId(Guid productId)
        {
            return await _dbContext.StockEntries
                .Where(s => s.ProductId == productId)
                .Include(s => s.CreatedBy)
                .ToListAsync();
        }

        public async Task<StockEntry?> GetById(Guid id)
        {
            return await _dbContext.StockEntries
                .Include(s => s.Product)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<StockEntry> Add(StockEntry stockEntry)
        {
            await _dbContext.StockEntries.AddAsync(stockEntry);
            return stockEntry;
        }
    }
}
