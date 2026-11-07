using Application.Dto.Paginacao;
using Application.Dto.SlaDto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.SlaValidator
{
    public class SlaRequestValidator : AbstractValidator<SlaRequestDto>
    {

        public SlaRequestValidator()
        {
           

            RuleFor(x => x.Prioridade)
                .NotEmpty()
                .WithMessage("Prioridade não pode ser vazio.");

            RuleFor(x => x.TempoAtendimentoHoras)
                .GreaterThan(0)
                .WithMessage("Tempo de atendimento deve ser maior que zero.");

            RuleFor(x => x.TempoResolucaoHoras)
                .GreaterThan(0)
                .WithMessage("Tempo de resolução deve ser maior que zero.");
        }


    }
}
