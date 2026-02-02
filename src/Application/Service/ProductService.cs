using Application.Dtos.Products;
using Application.Interfaces;
using Application.Service.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.Service
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductResponse>> GetAllProducts()
        {
            var products = await _unitOfWork.Products.GetAll();
            return products.Adapt<List<ProductResponse>>();
        }

        public async Task<ProductResponse?> GetProductById(Guid id)
        {
            var product = await _unitOfWork.Products.GetById(id);
            return product?.Adapt<ProductResponse>();
        }

        public async Task<ProductResponse> CreateProduct(ProductRequest request)
        {
            var newProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Products.Add(newProduct);
            await _unitOfWork.SaveChangesAsync();

            return newProduct.Adapt<ProductResponse>();
        }

        public async Task<bool> UpdateProduct(Guid id, ProductRequest request)
        {
            var existingProduct = await _unitOfWork.Products.GetById(id);
            
            if (existingProduct == null)
                return false;

            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProduct(Guid id)
        {
            var product = await _unitOfWork.Products.GetById(id);
            
            if (product == null)
                return false;

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
