using BolaoCopaMundo.Application.DTOs.Auth;
using FluentValidation;

namespace BolaoCopaMundo.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Formato de telefone inválido. Use o formato internacional, ex: +5511999999999.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.")
            .Matches(@"\d").WithMessage("Senha deve conter ao menos um número.");
    }
}
