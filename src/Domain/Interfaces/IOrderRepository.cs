using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAll();
        Task<IEnumerable<Order>> GetBySellerId(Guid sellerId);
        Task<Order?> GetById(Guid id);
        Task<Order> Add(Order order);
    }
}
