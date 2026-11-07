using Application.Dto.UsuarioDto;
using FluentValidation;

namespace Application.Validators.UsuarioValidator
{
    public class ChangePasswordValidator
        : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.PasswordAtual)
                .NotEmpty()
                .WithMessage("Senha atual é obrigatória.");

            RuleFor(x => x.NovaSenha)
                .NotEmpty()
                .WithMessage("Nova senha é obrigatória.")
                .MinimumLength(6)
                .WithMessage("A nova senha deve possuir no mínimo 6 caracteres.");

            RuleFor(x => x.ConfirmacaoNovaSenha)
                .Equal(x => x.NovaSenha)
                .WithMessage("As senhas não conferem.");
        }
    }
}