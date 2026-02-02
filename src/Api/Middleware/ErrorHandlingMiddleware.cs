using Application.Exceptions;
using System.Text.Json;

namespace Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse();

            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = validationEx.StatusCode;
                    errorResponse.ErrorCode = validationEx.ErrorCode;
                    errorResponse.Message = validationEx.Message;
                    errorResponse.Errors = validationEx.Errors;
                    break;

                case NotFoundException notFoundEx:
                    response.StatusCode = notFoundEx.StatusCode;
                    errorResponse.ErrorCode = notFoundEx.ErrorCode;
                    errorResponse.Message = notFoundEx.Message;
                    break;

                case InsufficientStockException stockEx:
                    response.StatusCode = stockEx.StatusCode;
                    errorResponse.ErrorCode = stockEx.ErrorCode;
                    errorResponse.Message = stockEx.Message;
                    errorResponse.Details = new
                    {
                        stockEx.ProductId,
                        stockEx.ProductName,
                        stockEx.RequestedQuantity,
                        stockEx.AvailableQuantity
                    };
                    break;

                case UnauthorizedException unauthorizedEx:
                    response.StatusCode = unauthorizedEx.StatusCode;
                    errorResponse.ErrorCode = unauthorizedEx.ErrorCode;
                    errorResponse.Message = unauthorizedEx.Message;
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = 401;
                    errorResponse.ErrorCode = "UNAUTHORIZED";
                    errorResponse.Message = "Credenciais inválidas ou token expirado.";
                    break;

                case BusinessException businessEx:
                    response.StatusCode = businessEx.StatusCode;
                    errorResponse.ErrorCode = businessEx.ErrorCode;
                    errorResponse.Message = businessEx.Message;
                    break;

                default:
                    response.StatusCode = 500;
                    errorResponse.ErrorCode = "INTERNAL_ERROR";
                    errorResponse.Message = "Ocorreu um erro interno no servidor.";
                    break;
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
        }
    }

    public class ErrorResponse
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public IDictionary<string, string[]>? Errors { get; set; }
        public object? Details { get; set; }
    }
}
