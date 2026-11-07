using Application.Dto.EmailNotification;
using Application.Dto.NotificacaoDto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.EmailNotificationValidator
{
    public class EmailNotificationRequestValidator : AbstractValidator<EmailNotificationRequestDto>
    {
        public EmailNotificationRequestValidator()
        {

            RuleFor(x => x.ChamadoId)
               .GreaterThan(0)
               .WithMessage("Chamado é obrigatório");

            RuleFor(x => x.Tipo)
             .NotEmpty()
             .WithMessage("Tipo é obrigatório");

            RuleFor(x => x.Destinatario)
             .NotEmpty()
             .WithMessage("Destinatario é obrigatório");

            RuleFor(x => x.Assunto)
             .NotEmpty()
             .WithMessage("Assunto é obrigatório");



        }



    }
}
