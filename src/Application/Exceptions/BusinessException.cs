namespace Application.Exceptions
{
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        public BusinessException(string message, string errorCode = "BUSINESS_ERROR", int statusCode = 400) 
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : BusinessException
    {
        public NotFoundException(string resource, object id) 
            : base($"{resource} com ID '{id}' não encontrado.", "NOT_FOUND", 404)
        {
        }
    }

    public class ValidationException : BusinessException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(string message, IDictionary<string, string[]>? errors = null) 
            : base(message, "VALIDATION_ERROR", 400)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }

    public class UnauthorizedException : BusinessException
    {
        public UnauthorizedException(string message = "Não autorizado") 
            : base(message, "UNAUTHORIZED", 401)
        {
        }
    }

    public class InsufficientStockException : BusinessException
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public int RequestedQuantity { get; }
        public int AvailableQuantity { get; }

        public InsufficientStockException(Guid productId, string productName, int requested, int available) 
            : base($"Estoque insuficiente para o produto '{productName}'. Solicitado: {requested}, Disponível: {available}.", 
                   "INSUFFICIENT_STOCK", 400)
        {
            ProductId = productId;
            ProductName = productName;
            RequestedQuantity = requested;
            AvailableQuantity = available;
        }
    }
}
