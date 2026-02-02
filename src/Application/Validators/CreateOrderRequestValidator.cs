using Application.Dtos.Orders;
using FluentValidation;

namespace Application.Validators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.DocumentType)
                .NotEmpty()
                .WithMessage("O tipo de documento é obrigatório.")
                .Must(type => type.ToUpperInvariant() == "CPF" || type.ToUpperInvariant() == "CNPJ")
                .WithMessage("O tipo de documento deve ser 'CPF' ou 'CNPJ'.");

            RuleFor(x => x.CustomerDocument)
                .NotEmpty()
                .WithMessage("O documento do cliente é obrigatório.")
                .Must(doc => doc.All(char.IsDigit))
                .WithMessage("O documento deve conter apenas números (sem pontos, traços ou barras).")
                .Must((request, doc) => HasValidDocumentLength(request.DocumentType, doc))
                .WithMessage(request => request.DocumentType.ToUpperInvariant() == "CPF" 
                    ? "CPF deve conter 11 dígitos." 
                    : "CNPJ deve conter 14 dígitos.");

            RuleFor(x => x.Items)
                .NotNull()
                .WithMessage("Os itens do pedido são obrigatórios.")
                .NotEmpty()
                .WithMessage("O pedido deve conter pelo menos um item.");

            RuleForEach(x => x.Items)
                .SetValidator(new OrderItemRequestValidator());
        }

        private static bool HasValidDocumentLength(string documentType, string document)
        {
            if (string.IsNullOrEmpty(document)) return false;
            
            return documentType.ToUpperInvariant() switch
            {
                "CPF" => document.Length == 11,
                "CNPJ" => document.Length == 14,
                _ => false
            };
        }
    }

    public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
    {
        public OrderItemRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("O produto do item é obrigatório.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("A quantidade de cada item deve ser maior que zero.");
        }
    }
}
