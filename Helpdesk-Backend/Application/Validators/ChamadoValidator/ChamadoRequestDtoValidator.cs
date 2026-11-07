using Application.Dto.Chamado;
using FluentValidation;

namespace Application.Validators.ChamadoValidator
{
    public class ChamadoRequestDtoValidator
        : AbstractValidator<ChamadoRequestDto>
    {
        public ChamadoRequestDtoValidator()
        {
            RuleFor(x => x.CategoriaAtendimentoId)
                .GreaterThan(0)
                .WithMessage("Categoria é obrigatória");

            RuleFor(x => x.EquipamentoAfetado)
                .NotEmpty()
                .WithMessage("Equipamento afetado é obrigatório");

            RuleFor(x => x.Descricao)
                .NotEmpty()
                .WithMessage("Descrição é obrigatória");
        }
    }
}