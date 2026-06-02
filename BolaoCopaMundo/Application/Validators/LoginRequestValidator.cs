using BolaoCopaMundo.Application.DTOs.Auth;
using FluentValidation;

namespace BolaoCopaMundo.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Telefone é obrigatório.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
