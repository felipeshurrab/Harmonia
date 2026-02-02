using Application.Dtos.Orders;

namespace Application.Service.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponse>> GetAllOrders();
        Task<IEnumerable<OrderResponse>> GetOrdersBySellerId(Guid sellerId);
        Task<OrderResponse?> GetOrderById(Guid id);
        Task<OrderResponse> CreateOrderWithStockValidation(CreateOrderRequest request, Guid sellerId, string sellerName);
    }
}
