using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAll();
        Task<Product?> GetById(Guid id);
        Task<Product> Add(Product product);
        void Update(Product product);
        void Delete(Product product);
    }
}
