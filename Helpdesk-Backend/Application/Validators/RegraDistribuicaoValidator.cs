using Application.DTOs;
using Domain.Enum;
using FluentValidation;

namespace Application.Validations
{
    public class RegraDistribuicaoRequestValidator : AbstractValidator<RegraDistribuicaoRequestDto>
    {
        public RegraDistribuicaoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty()
                .WithMessage("Nome da regra é obrigatório.");

            RuleFor(x => x.TipoRegra)
                .IsInEnum()
                .WithMessage("Tipo de regra inválido.");

            RuleFor(x => x.TipoDestino)
                .IsInEnum()
                .WithMessage("Tipo de destino inválido.");

            RuleFor(x => x.UsuarioDestinoId)
                .NotEmpty()
                .When(x => x.TipoDestino == TipoDestinoEnum.Usuario)
                .WithMessage("Usuário destino é obrigatório.");

            RuleFor(x => x.PerfilDestinoId)
                .NotNull()
                .GreaterThan(0)
                .When(x => x.TipoDestino == TipoDestinoEnum.Perfil)
                .WithMessage("Perfil destino é obrigatório.");

            RuleFor(x => x.DataInicial)
                .NotNull()
                .When(x => !x.PeriodoIndefinido)
                .WithMessage("Data inicial é obrigatória.");

            RuleFor(x => x.DataFinal)
                .NotNull()
                .When(x => !x.PeriodoIndefinido)
                .WithMessage("Data final é obrigatória.");

            RuleFor(x => x)
                .Must(x =>
                    x.PeriodoIndefinido ||
                    (x.DataInicial.HasValue &&
                     x.DataFinal.HasValue &&
                     x.DataFinal >= x.DataInicial))
                .WithMessage("Data final deve ser maior ou igual à data inicial.");
        }
    }
}