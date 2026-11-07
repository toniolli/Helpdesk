using Application.Dto.Comentario;
using FluentValidation;

namespace Application.Validators.ComentarioValidator
{
    public class ComentarioRequestDtoValidator
        : AbstractValidator<ComentarioRequestDto>
    {
        public ComentarioRequestDtoValidator()
        {
            RuleFor(x => x.ChamadoId)
                .GreaterThan(0)
                .WithMessage("Chamado é obrigatório");

            RuleFor(x => x.Mensagem)
                .MaximumLength(4000)
                .When(x => !string.IsNullOrWhiteSpace(x.Mensagem))
                .WithMessage("A mensagem não pode exceder 4000 caracteres.");

            RuleFor(x => x.Arquivos)
               .NotNull()
               .WithMessage("A lista de arquivos é obrigatória.")
               .Must(arquivos => arquivos == null || arquivos.Count <= 5)
               .WithMessage(
                   "É permitido enviar no máximo 5 arquivos por comentário.");

            RuleFor(x => x)
               .Must(x =>
                   !string.IsNullOrWhiteSpace(x.Mensagem) ||
                   (x.Arquivos != null && x.Arquivos.Count > 0))
               .WithMessage(
                   "Informe uma mensagem ou adicione pelo menos um arquivo.");

        }
    }
}