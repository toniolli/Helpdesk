using Application.Dto.Chamado;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators.ChamadoValidator
{
    public class FecharChamadoDtoValidator
        : AbstractValidator<FecharChamadoDto>
    {
        public FecharChamadoDtoValidator()
        {
            RuleFor(x => x.TipoAtendimento)
                .IsInEnum()
                .WithMessage("Tipo de atendimento inválido");

            RuleFor(x => x.TipoEncerramento)
                .IsInEnum()
                .WithMessage("Tipo de encerramento inválido");

            When(x => x.TipoEncerramento == TipoEncerramentoEnum.Resolvido, () =>
            {
                RuleFor(x => x.Solucao)
                    .NotEmpty()
                    .WithMessage("Solução é obrigatória para chamados resolvidos");
            });
        }
    }
}