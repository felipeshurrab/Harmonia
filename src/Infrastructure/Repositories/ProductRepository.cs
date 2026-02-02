using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly HarmoniaDbContext _dbContext;

        public ProductRepository(HarmoniaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<Product?> GetById(Guid id)
        {
            return await _dbContext.Products.FindAsync(id);
        }

        public async Task<Product> Add(Product product)
        {
            await _dbContext.Products.AddAsync(product);
            return product;
        }

        public void Update(Product product)
        {
            _dbContext.Products.Update(product);
        }

        public void Delete(Product product)
        {
            _dbContext.Products.Remove(product);
        }
    }
}
