using Application.Dtos.Auth;
using FluentValidation;

namespace Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        private static readonly string[] ValidUserRoles = { "Administrator", "Seller" };

        public RegisterRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome é obrigatório.")
                .MinimumLength(2)
                .WithMessage("O nome deve ter pelo menos 2 caracteres.")
                .MaximumLength(100)
                .WithMessage("O nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("O e-mail é obrigatório.")
                .EmailAddress()
                .WithMessage("Informe um e-mail válido.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("A senha é obrigatória.")
                .MinimumLength(6)
                .WithMessage("A senha deve ter no mínimo 6 caracteres.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("O tipo de usuário é obrigatório.")
                .Must(IsValidUserRole)
                .WithMessage("Tipo de usuário inválido. Use 'Administrator' ou 'Seller'.");
        }

        private static bool IsValidUserRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;
            return ValidUserRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }
    }
}
