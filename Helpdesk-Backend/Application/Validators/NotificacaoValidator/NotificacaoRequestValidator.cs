using Application.Dto.Comentario;
using Application.Dto.NotificacaoDto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.NotificacaoValidator
{
    public class NotificacaoRequestValidator : AbstractValidator<NotificacaoRequestDto>
    {

        public NotificacaoRequestValidator()
        {
            
        
        RuleFor(x => x.chamadoId)
                .GreaterThan(0)
                .WithMessage("Chamado é obrigatório");

        RuleFor(x => x.usuarioId)
                .NotEmpty()
                .WithMessage("Usuário é obrigatório");

        RuleFor(x => x.titulo)
               .NotEmpty()
               .WithMessage("Tirulo é obrigatório");


        RuleFor(x => x.mensagem)
               .NotEmpty()
               .WithMessage("Mensagem é obrigatório");

        RuleFor(x => x.tipo)
               .NotEmpty()
               .WithMessage("Tipo é obrigatório"); 
            
        RuleFor(x => x.status)
               .NotEmpty()
               .WithMessage("Status é obrigatório");
        }
    }
}
