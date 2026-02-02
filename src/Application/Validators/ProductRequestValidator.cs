using Application.Dtos.Products;
using FluentValidation;

namespace Application.Validators
{
    public class ProductRequestValidator : AbstractValidator<ProductRequest>
    {
        public ProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome do produto é obrigatório.")
                .MinimumLength(2)
                .WithMessage("O nome deve ter pelo menos 2 caracteres.")
                .MaximumLength(200)
                .WithMessage("O nome deve ter no máximo 200 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("A descrição deve ter no máximo 1000 caracteres.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("O preço deve ser maior que zero.");
        }
    }
}
