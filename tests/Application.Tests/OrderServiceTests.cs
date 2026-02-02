using Application.Dtos.Orders;
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
    public class OrderServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderServiceTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _orderRepository = A.Fake<IOrderRepository>();
            _productRepository = A.Fake<IProductRepository>();
            A.CallTo(() => _unitOfWork.Orders).Returns(_orderRepository);
            A.CallTo(() => _unitOfWork.Products).Returns(_productRepository);
        }

        [Fact]
        public async Task CreateOrderWithStockValidation_Com_Estoque_Suficiente_Cria_Pedido_E_Deduz_Estoque()
        {
            var productId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var produto = new Product
            {
                Id = productId,
                Name = "Coleira Premium",
                Price = 89.90m,
                StockQuantity = 100
            };

            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "12345678900",
                Items = new List<OrderItemRequest>
                {
                    new() { ProductId = productId, Quantity = 5 }
                }
            };

            A.CallTo(() => _productRepository.GetById(productId)).Returns(produto);
            A.CallTo(() => _orderRepository.Add(A<Order>._)).ReturnsLazily((Order o) => o);
            A.CallTo(() => _orderRepository.GetById(A<Guid>._)).Returns(new Order
            {
                Id = Guid.NewGuid(),
                CustomerDocumentType = Domain.Enums.DocumentType.CPF,
                CustomerDocument = "12345678900",
                SellerName = "João",
                TotalAmount = 449.50m,
                Items = new List<OrderItem>
                {
                    new() { ProductId = productId, Product = produto, Quantity = 5, UnitPrice = 89.90m }
                }
            });

            var service = new OrderService(_unitOfWork);

            var result = await service.CreateOrderWithStockValidation(request, sellerId, "João");

            Assert.NotNull(result);
            Assert.Equal("CPF", result.DocumentType);
            Assert.Equal("12345678900", result.CustomerDocument);
            Assert.Equal(449.50m, result.TotalAmount);
            Assert.Equal(95, produto.StockQuantity);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(default)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CreateOrderWithStockValidation_Com_Estoque_Insuficiente_Lanca_InsufficientStockException()
        {
            var productId = Guid.NewGuid();
            var produto = new Product
            {
                Id = productId,
                Name = "Coleira Premium",
                Price = 89.90m,
                StockQuantity = 3
            };

            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "12345678900",
                Items = new List<OrderItemRequest>
                {
                    new() { ProductId = productId, Quantity = 10 }
                }
            };

            A.CallTo(() => _productRepository.GetById(productId)).Returns(produto);

            var service = new OrderService(_unitOfWork);

            var exception = await Assert.ThrowsAsync<InsufficientStockException>(
                () => service.CreateOrderWithStockValidation(request, Guid.NewGuid(), "João"));

            Assert.Equal(productId, exception.ProductId);
            Assert.Equal("Coleira Premium", exception.ProductName);
            Assert.Equal(10, exception.RequestedQuantity);
            Assert.Equal(3, exception.AvailableQuantity);
        }

        [Fact]
        public async Task CreateOrderWithStockValidation_Com_Produto_Inexistente_Lanca_NotFoundException()
        {
            var productId = Guid.NewGuid();
            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "12345678900",
                Items = new List<OrderItemRequest>
                {
                    new() { ProductId = productId, Quantity = 1 }
                }
            };

            A.CallTo(() => _productRepository.GetById(productId)).Returns((Product?)null);

            var service = new OrderService(_unitOfWork);

            await Assert.ThrowsAsync<NotFoundException>(
                () => service.CreateOrderWithStockValidation(request, Guid.NewGuid(), "João"));
        }

        [Fact]
        public async Task CreateOrderWithStockValidation_Com_Multiplos_Itens_Calcula_Total_Corretamente()
        {
            var product1Id = Guid.NewGuid();
            var product2Id = Guid.NewGuid();

            var produto1 = new Product { Id = product1Id, Name = "Coleira", Price = 50m, StockQuantity = 100 };
            var produto2 = new Product { Id = product2Id, Name = "Brinquedo", Price = 30m, StockQuantity = 100 };

            var request = new CreateOrderRequest
            {
                DocumentType = "CNPJ",
                CustomerDocument = "12345678000190",
                Items = new List<OrderItemRequest>
                {
                    new() { ProductId = product1Id, Quantity = 2 },
                    new() { ProductId = product2Id, Quantity = 3 }
                }
            };

            A.CallTo(() => _productRepository.GetById(product1Id)).Returns(produto1);
            A.CallTo(() => _productRepository.GetById(product2Id)).Returns(produto2);
            A.CallTo(() => _orderRepository.Add(A<Order>._)).ReturnsLazily((Order o) => o);
            A.CallTo(() => _orderRepository.GetById(A<Guid>._)).Returns(new Order
            {
                TotalAmount = 190m,
                CustomerDocumentType = Domain.Enums.DocumentType.CNPJ,
                CustomerDocument = "12345678000190",
                SellerName = "João",
                Items = new List<OrderItem>()
            });

            var service = new OrderService(_unitOfWork);

            var result = await service.CreateOrderWithStockValidation(request, Guid.NewGuid(), "João");

            Assert.Equal(190m, result.TotalAmount);
            Assert.Equal(98, produto1.StockQuantity);
            Assert.Equal(97, produto2.StockQuantity);
        }
    }

    public class CreateOrderRequestValidatorTests
    {
        private readonly CreateOrderRequestValidator _validator = new();

        [Fact]
        public void Valida_DocumentType_Obrigatorio()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "",
                CustomerDocument = "12345678900",
                Items = new List<OrderItemRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
            };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.DocumentType);
        }

        [Fact]
        public void Valida_DocumentType_Invalido()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "RG",
                CustomerDocument = "12345678900",
                Items = new List<OrderItemRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
            };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.DocumentType)
                .WithErrorMessage("O tipo de documento deve ser 'CPF' ou 'CNPJ'.");
        }

        [Fact]
        public void Valida_Documento_Obrigatorio()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "",
                Items = new List<OrderItemRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
            };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.CustomerDocument);
        }

        [Fact]
        public void Valida_CPF_Com_11_Digitos()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "1234567890",
                Items = new List<OrderItemRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
            };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.CustomerDocument)
                .WithErrorMessage("CPF deve conter 11 dígitos.");
        }

        [Fact]
        public void Valida_CNPJ_Com_14_Digitos()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "CNPJ",
                CustomerDocument = "1234567890123",
                Items = new List<OrderItemRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
            };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.CustomerDocument)
                .WithErrorMessage("CNPJ deve conter 14 dígitos.");
        }

        [Fact]
        public void Valida_Documento_Apenas_Numeros()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "123.456.789-00",
                Items = new List<OrderItemRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
            };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.CustomerDocument)
                .WithErrorMessage("O documento deve conter apenas números (sem pontos, traços ou barras).");
        }

        [Fact]
        public void Valida_Itens_Obrigatorios()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "12345678900",
                Items = new List<OrderItemRequest>()
            };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Items)
                .WithErrorMessage("O pedido deve conter pelo menos um item.");
        }

        [Fact]
        public void Valida_Item_Quantidade_Maior_Que_Zero()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "12345678900",
                Items = new List<OrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 0 }
                }
            };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor("Items[0].Quantity");
        }

        [Fact]
        public void Aceita_CPF_Valido()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "CPF",
                CustomerDocument = "12345678900",
                Items = new List<OrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 5 }
                }
            };
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Aceita_CNPJ_Valido()
        {
            var request = new CreateOrderRequest
            {
                DocumentType = "CNPJ",
                CustomerDocument = "12345678000190",
                Items = new List<OrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 5 }
                }
            };
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
