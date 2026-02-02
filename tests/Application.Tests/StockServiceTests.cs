using Application.Dtos.Stock;
using Application.Exceptions;
using Application.Interfaces;
using Application.Service;
using Application.Validators;
using Domain.Entities;
using Domain.Interfaces;
using FakeItEasy;
using FluentValidation.TestHelper;

namespace Application.Tests
{
    public class StockServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockEntryRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public StockServiceTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _stockRepository = A.Fake<IStockEntryRepository>();
            _productRepository = A.Fake<IProductRepository>();
            A.CallTo(() => _unitOfWork.StockEntries).Returns(_stockRepository);
            A.CallTo(() => _unitOfWork.Products).Returns(_productRepository);
        }

        [Fact]
        public async Task AddStockToProduct_Com_Request_Valido_Atualiza_Quantidade_Do_Produto()
        {
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var produto = new Product
            {
                Id = productId,
                Name = "Coleira",
                StockQuantity = 10
            };

            var request = new StockEntryRequest
            {
                ProductId = productId,
                Quantity = 50,
                InvoiceNumber = "NF-001"
            };

            A.CallTo(() => _productRepository.GetById(productId)).Returns(produto);
            A.CallTo(() => _stockRepository.Add(A<StockEntry>._)).ReturnsLazily((StockEntry s) => s);
            A.CallTo(() => _stockRepository.GetById(A<Guid>._)).Returns(new StockEntry
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Product = produto,
                Quantity = 50,
                InvoiceNumber = "NF-001",
                CreatedBy = new User { Name = "Admin" }
            });

            var service = new StockService(_unitOfWork);

            var result = await service.AddStockToProduct(request, userId);

            Assert.NotNull(result);
            Assert.Equal(50, result.Quantity);
            Assert.Equal("NF-001", result.InvoiceNumber);
            Assert.Equal(60, produto.StockQuantity);
            A.CallTo(() => _productRepository.Update(A<Product>.That.Matches(p => p.StockQuantity == 60))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task AddStockToProduct_Com_Produto_Inexistente_Lanca_NotFoundException()
        {
            var productId = Guid.NewGuid();
            var request = new StockEntryRequest
            {
                ProductId = productId,
                Quantity = 10,
                InvoiceNumber = "NF-001"
            };

            A.CallTo(() => _productRepository.GetById(productId)).Returns((Product?)null);

            var service = new StockService(_unitOfWork);

            await Assert.ThrowsAsync<NotFoundException>(() => service.AddStockToProduct(request, Guid.NewGuid()));
        }
    }

    public class StockEntryRequestValidatorTests
    {
        private readonly StockEntryRequestValidator _validator = new();

        [Fact]
        public void Valida_Quantidade_Maior_Que_Zero()
        {
            var request = new StockEntryRequest { ProductId = Guid.NewGuid(), Quantity = 0, InvoiceNumber = "NF-001" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                .WithErrorMessage("A quantidade deve ser maior que zero.");
        }

        [Fact]
        public void Valida_Quantidade_Negativa()
        {
            var request = new StockEntryRequest { ProductId = Guid.NewGuid(), Quantity = -5, InvoiceNumber = "NF-001" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Quantity);
        }

        [Fact]
        public void Valida_NotaFiscal_Obrigatoria()
        {
            var request = new StockEntryRequest { ProductId = Guid.NewGuid(), Quantity = 10, InvoiceNumber = "" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.InvoiceNumber)
                .WithErrorMessage("O número da nota fiscal é obrigatório para fins de auditoria.");
        }

        [Fact]
        public void Valida_ProductId_Obrigatorio()
        {
            var request = new StockEntryRequest { ProductId = Guid.Empty, Quantity = 10, InvoiceNumber = "NF-001" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.ProductId);
        }

        [Fact]
        public void Aceita_Dados_Validos()
        {
            var request = new StockEntryRequest { ProductId = Guid.NewGuid(), Quantity = 100, InvoiceNumber = "NF-2026-001" };
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
