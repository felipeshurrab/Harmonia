namespace Application.Dtos.Stock
{
    public class StockEntryResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
    }
}
