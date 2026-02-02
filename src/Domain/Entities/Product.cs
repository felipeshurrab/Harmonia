using Domain.Common;

namespace Domain.Entities
{
    public class Product : EntityBase
    {
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public int StockQuantity { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
