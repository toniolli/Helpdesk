using Application.Dto.UsuarioDto;
using FluentValidation;

namespace Application.Validators.UsuarioValidator
{
    public class LoginValidator
        : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Usuário é obrigatório.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Senha é obrigatória.");
        }
    }
}