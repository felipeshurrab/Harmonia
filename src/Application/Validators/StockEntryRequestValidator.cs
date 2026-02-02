using Application.Dtos.Stock;
using FluentValidation;

namespace Application.Validators
{
    public class StockEntryRequestValidator : AbstractValidator<StockEntryRequest>
    {
        public StockEntryRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("O produto é obrigatório.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("A quantidade deve ser maior que zero.");

            RuleFor(x => x.InvoiceNumber)
                .NotEmpty()
                .WithMessage("O número da nota fiscal é obrigatório para fins de auditoria.")
                .MaximumLength(50)
                .WithMessage("O número da nota fiscal deve ter no máximo 50 caracteres.");
        }
    }
}
