using Application.Dto.Paginacao;
using FluentValidation;

namespace Application.Validators.Paginacao
{
    public class PaginacaoRequestDtoValidator
        : AbstractValidator<PaginacaoRequestDto>
    {
        public PaginacaoRequestDtoValidator()
        {
            RuleFor(x => x.Pagina)
                .GreaterThan(0)
                .WithMessage("A página deve ser maior que zero.");

            RuleFor(x => x.TamanhoPagina)
                .GreaterThan(0)
                .WithMessage("O tamanho da página deve ser maior que zero.");

            RuleFor(x => x.TamanhoPagina)
                .LessThanOrEqualTo(100)
                .WithMessage("O tamanho máximo da página é 100.");
        }
    }
}