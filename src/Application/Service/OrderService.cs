using Application.Dtos.Orders;
using Application.Exceptions;
using Application.Interfaces;
using Application.Service.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderResponse>> GetAllOrders()
        {
            var orders = await _unitOfWork.Orders.GetAll();
            return orders.Select(MapToOrderResponse);
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersBySellerId(Guid sellerId)
        {
            var orders = await _unitOfWork.Orders.GetBySellerId(sellerId);
            return orders.Select(MapToOrderResponse);
        }

        public async Task<OrderResponse?> GetOrderById(Guid id)
        {
            var order = await _unitOfWork.Orders.GetById(id);
            return order == null ? null : MapToOrderResponse(order);
        }

        public async Task<OrderResponse> CreateOrderWithStockValidation(CreateOrderRequest request, Guid sellerId, string sellerName)
        {
            var orderId = Guid.NewGuid();
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemRequest in request.Items)
            {
                var product = await _unitOfWork.Products.GetById(itemRequest.ProductId);
                if (product == null)
                    throw new NotFoundException("Produto", itemRequest.ProductId);

                if (product.StockQuantity < itemRequest.Quantity)
                {
                    throw new InsufficientStockException(
                        product.Id, 
                        product.Name, 
                        itemRequest.Quantity, 
                        product.StockQuantity
                    );
                }

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = product.Id,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.Quantity * orderItem.UnitPrice;
            }

            var newOrder = new Order
            {
                Id = orderId,
                CustomerDocumentType = Enum.Parse<DocumentType>(request.DocumentType, ignoreCase: true),
                CustomerDocument = request.CustomerDocument,
                SellerName = sellerName,
                SellerId = sellerId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Items = orderItems
            };

            foreach (var itemRequest in request.Items)
            {
                var product = await _unitOfWork.Products.GetById(itemRequest.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= itemRequest.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Products.Update(product);
                }
            }

            await _unitOfWork.Orders.Add(newOrder);
            await _unitOfWork.SaveChangesAsync();

            var savedOrder = await _unitOfWork.Orders.GetById(newOrder.Id);
            return MapToOrderResponse(savedOrder!);
        }

        private static OrderResponse MapToOrderResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                DocumentType = order.CustomerDocumentType.ToString(),
                CustomerDocument = order.CustomerDocument,
                SellerName = order.SellerName,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                }).ToList()
            };
        }
    }
}
