using Application.Dtos.Products;
using Application.Interfaces;
using Application.Service;
using Application.Validators;
using Domain.Entities;
using Domain.Interfaces;
using FakeItEasy;
using FluentValidation.TestHelper;

namespace Application.Tests
{
    public class ProductServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;

        public ProductServiceTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _productRepository = A.Fake<IProductRepository>();
            A.CallTo(() => _unitOfWork.Products).Returns(_productRepository);
        }

        [Fact]
        public async Task GetAllProducts_Retorna_Lista_De_Produtos()
        {
            var produtos = new List<Product>
            {
                new() { Id = Guid.NewGuid(), Name = "Coleira Premium", Price = 89.90m },
                new() { Id = Guid.NewGuid(), Name = "Brinquedo para Gatos", Price = 29.90m }
            };
            A.CallTo(() => _productRepository.GetAll()).Returns(produtos);
            var service = new ProductService(_unitOfWork);

            var result = await service.GetAllProducts();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetProductById_Com_Id_Valido_Retorna_Produto()
        {
            var productId = Guid.NewGuid();
            var produto = new Product { Id = productId, Name = "Coleira Premium", Price = 89.90m };
            A.CallTo(() => _productRepository.GetById(productId)).Returns(produto);
            var service = new ProductService(_unitOfWork);

            var result = await service.GetProductById(productId);

            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Coleira Premium", result.Name);
        }

        [Fact]
        public async Task GetProductById_Com_Id_Invalido_Retorna_Null()
        {
            var productId = Guid.NewGuid();
            A.CallTo(() => _productRepository.GetById(productId)).Returns((Product?)null);
            var service = new ProductService(_unitOfWork);

            var result = await service.GetProductById(productId);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateProduct_Com_Request_Valido_Retorna_Produto()
        {
            var request = new ProductRequest
            {
                Name = "Cama para Cachorro",
                Description = "Cama confortável tamanho G",
                Price = 199.90m
            };
            A.CallTo(() => _productRepository.Add(A<Product>._)).ReturnsLazily((Product p) => p);
            var service = new ProductService(_unitOfWork);

            var result = await service.CreateProduct(request);

            Assert.NotNull(result);
            Assert.Equal("Cama para Cachorro", result.Name);
            Assert.Equal(199.90m, result.Price);
            Assert.Equal(0, result.StockQuantity);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(default)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateProduct_Com_Dados_Validos_Retorna_True()
        {
            var productId = Guid.NewGuid();
            var produtoExistente = new Product { Id = productId, Name = "Nome Antigo", Price = 50m };
            A.CallTo(() => _productRepository.GetById(productId)).Returns(produtoExistente);

            var request = new ProductRequest { Name = "Nome Novo", Price = 75m };
            var service = new ProductService(_unitOfWork);

            var result = await service.UpdateProduct(productId, request);

            Assert.True(result);
            A.CallTo(() => _productRepository.Update(A<Product>.That.Matches(p => p.Name == "Nome Novo"))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(default)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateProduct_Com_Produto_Inexistente_Retorna_False()
        {
            var productId = Guid.NewGuid();
            A.CallTo(() => _productRepository.GetById(productId)).Returns((Product?)null);

            var request = new ProductRequest { Name = "Nome Novo", Price = 75m };
            var service = new ProductService(_unitOfWork);

            var result = await service.UpdateProduct(productId, request);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProduct_Com_Produto_Existente_Retorna_True()
        {
            var productId = Guid.NewGuid();
            var produtoExistente = new Product { Id = productId, Name = "Para Deletar" };
            A.CallTo(() => _productRepository.GetById(productId)).Returns(produtoExistente);

            var service = new ProductService(_unitOfWork);

            var result = await service.DeleteProduct(productId);

            Assert.True(result);
            A.CallTo(() => _productRepository.Delete(produtoExistente)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(default)).MustHaveHappenedOnceExactly();
        }
    }

    public class ProductRequestValidatorTests
    {
        private readonly ProductRequestValidator _validator = new();

        [Fact]
        public void Valida_Nome_Obrigatorio()
        {
            var request = new ProductRequest { Name = "", Price = 50m };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Valida_Preco_Maior_Que_Zero()
        {
            var request = new ProductRequest { Name = "Produto", Price = 0 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Price)
                .WithErrorMessage("O preço deve ser maior que zero.");
        }

        [Fact]
        public void Valida_Preco_Negativo()
        {
            var request = new ProductRequest { Name = "Produto", Price = -10 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Price);
        }

        [Fact]
        public void Aceita_Dados_Validos()
        {
            var request = new ProductRequest { Name = "Coleira Premium", Description = "Descrição", Price = 89.90m };
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
