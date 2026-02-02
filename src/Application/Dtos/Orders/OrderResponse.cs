namespace Application.Dtos.Orders
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string CustomerDocument { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }

    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
