using Domain.Validation;

namespace Domain.Entities
{
    // Representa o vínculo entre um perfil e uma permissão do sistema.
    public class PerfilPermissao
    {
        public int PerfilId { get; private set; }
        public int PermissaoId { get; private set; }
        public Perfil Perfil { get; private set; }
        public Permissao Permissao { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected PerfilPermissao()
        {
        }

        public PerfilPermissao(
            int perfilId,
            int permissaoId)
        {
            ValidateDomain(
                perfilId,
                permissaoId);

            PerfilId = perfilId;
            PermissaoId = permissaoId;
        }

        // Impede a criação de vínculos com identificadores inválidos.
        private void ValidateDomain(
            int perfilId,
            int permissaoId)
        {
            DomainExceptionValidation.when(
                perfilId <= 0,
                "Perfil inválido");

            DomainExceptionValidation.when(
                permissaoId <= 0,
                "Permissão inválida");
        }
    }
}
