using Application.DTOs;
using FluentValidation;

namespace Application.Validations
{
    public class CriterioDistribuicaoRequestValidator : AbstractValidator<CriterioDistribuicaoRequestDto>
    {
        public CriterioDistribuicaoRequestValidator()
        {
            RuleFor(x => x.RegraDistribuicaoId)
                .GreaterThan(0)
                .WithMessage("Regra de distribuição é obrigatória.");

            RuleFor(x => x.CategoriaAtendimentoId)
                .GreaterThan(0)
                .WithMessage("Categoria de atendimento é obrigatória.");
        }
    }
}