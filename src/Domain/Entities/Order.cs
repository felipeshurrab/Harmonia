using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Order : EntityBase
    {
        public DocumentType CustomerDocumentType { get; set; }
        
        public string CustomerDocument { get; set; } = string.Empty;
        
        public string SellerName { get; set; } = string.Empty;
        
        public Guid SellerId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public decimal TotalAmount { get; set; }
        
        public User Seller { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
