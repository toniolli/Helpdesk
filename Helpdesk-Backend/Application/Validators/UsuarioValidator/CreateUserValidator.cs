using Application.Dto.UsuarioDto;
using FluentValidation;

namespace Application.Validators.UsuarioValidator
{
    public class CreateUserValidator
        : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Nome de usuário é obrigatório.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email é obrigatório.")
                .EmailAddress()
                .WithMessage("Email inválido.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Senha é obrigatória.")
                .MinimumLength(6)
                .WithMessage("A senha deve possuir no mínimo 6 caracteres.");

            RuleFor(x => x.PasswordConfirmation)
                .NotEmpty()
                .WithMessage("Confirmação da senha é obrigatória.")
                .Equal(x => x.Password)
                .WithMessage("As senhas não conferem.");
        }
    }
}