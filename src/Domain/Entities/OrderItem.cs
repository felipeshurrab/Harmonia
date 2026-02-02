using Domain.Common;

namespace Domain.Entities
{
    public class OrderItem : EntityBase
    {
        public Guid OrderId { get; set; }
        
        public Guid ProductId { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice => Quantity * UnitPrice;
        
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
