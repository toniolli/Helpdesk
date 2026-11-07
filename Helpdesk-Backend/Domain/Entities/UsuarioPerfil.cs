using Domain.Entities;
using Domain.Validation;

namespace Domain.Entities
{
    // Representa o vínculo entre um usuário do Identity e um perfil da aplicação.
    public class UsuarioPerfil
    {
        public string UsuarioId { get; private set; }
        public int PerfilId { get; private set; }
        public Perfil Perfil { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected UsuarioPerfil()
        {
        }

        public UsuarioPerfil(
            string usuarioId,
            int perfilId)
        {
            ValidateDomain(
                usuarioId,
                perfilId);

            UsuarioId = usuarioId;
            PerfilId = perfilId;
        }

        // Impede a criação de vínculos sem usuário ou perfil válido.
        private void ValidateDomain(
            string usuarioId,
            int perfilId)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(usuarioId),
                "Usuário inválido");

            DomainExceptionValidation.when(
                perfilId <= 0,
                "Perfil inválido");
        }
    }
}