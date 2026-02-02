namespace Application.Dtos.Orders
{
    public class CreateOrderRequest
    {
        public string DocumentType { get; set; } = string.Empty;
        public string CustomerDocument { get; set; } = string.Empty;
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
