namespace Application.Dtos.Stock
{
    public class StockEntryRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
    }
}
