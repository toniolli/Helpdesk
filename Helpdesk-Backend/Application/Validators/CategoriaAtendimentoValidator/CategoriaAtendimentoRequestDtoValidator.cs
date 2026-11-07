using Application.Dto.CategoriaAtendimentoDto;
using FluentValidation;

namespace Application.Validators.CategoriaAtendimentoValidator
{
    public class CategoriaAtendimentoRequestDtoValidator
        : AbstractValidator<CategoriaAtendimentoRequestDto>
    {
        public CategoriaAtendimentoRequestDtoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty()
                .WithMessage("Nome é obrigatório");

            RuleFor(x => x.Prioridade)
                .IsInEnum()
                .WithMessage("Prioridade inválida");
        }
    }
}