using BolaoCopaMundo.Application.DTOs.BolaoGroup;
using FluentValidation;

namespace BolaoCopaMundo.Application.Validators;

public class CreateBolaoGroupValidator : AbstractValidator<CreateBolaoGroupRequest>
{
    public CreateBolaoGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do grupo é obrigatório.")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres.")
            .When(x => x.Description is not null);
    }
}
