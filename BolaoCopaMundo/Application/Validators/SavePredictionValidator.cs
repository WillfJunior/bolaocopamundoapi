using BolaoCopaMundo.Application.DTOs.Prediction;
using FluentValidation;

namespace BolaoCopaMundo.Application.Validators;

public class SavePredictionValidator : AbstractValidator<SavePredictionRequest>
{
    public SavePredictionValidator()
    {
        RuleFor(x => x.MatchId).GreaterThan(0).WithMessage("Jogo inválido.");
        RuleFor(x => x.HomeScore).GreaterThanOrEqualTo(0).WithMessage("Placar do mandante não pode ser negativo.");
        RuleFor(x => x.AwayScore).GreaterThanOrEqualTo(0).WithMessage("Placar do visitante não pode ser negativo.");
    }
}
