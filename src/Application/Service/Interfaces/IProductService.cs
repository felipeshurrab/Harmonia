using Application.Dtos.Products;

namespace Application.Service.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllProducts();
        Task<ProductResponse?> GetProductById(Guid id);
        Task<ProductResponse> CreateProduct(ProductRequest request);
        Task<bool> UpdateProduct(Guid id, ProductRequest request);
        Task<bool> DeleteProduct(Guid id);
    }
}
