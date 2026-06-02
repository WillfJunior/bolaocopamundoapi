using BolaoCopaMundo.Application.DTOs.Auth;
using FluentValidation;

namespace BolaoCopaMundo.Application.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Senha atual é obrigatória.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nova senha é obrigatória.")
            .MinimumLength(8).WithMessage("Nova senha deve ter no mínimo 8 caracteres.")
            .Matches(@"\d").WithMessage("Nova senha deve conter ao menos um número.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Nova senha deve ser diferente da atual.");
    }
}
