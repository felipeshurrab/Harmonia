using Domain.Common;

namespace Domain.Entities
{
    public class StockEntry : EntityBase
    {
        public Guid ProductId { get; set; }
        
        public int Quantity { get; set; }
        
        public string InvoiceNumber { get; set; } = string.Empty;
        
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        
        public Guid CreatedByUserId { get; set; }
        
        public Product Product { get; set; } = null!;
        public User CreatedBy { get; set; } = null!;
    }
}
