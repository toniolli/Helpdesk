using Domain.Enum;

namespace Infra.Identity
{
    // Representa uma sessão de autenticação renovável.
    // O refresh token original nunca é salvo; somente seu hash.
    public class SessaoRefresh
    {
        public Guid Id { get; private set; }
        public string UsuarioId { get; private set; }
        public string TokenHash { get; private set; }
        public TipoSessaoAutenticacaoEnum Tipo { get; private set; }
        public DateTime CriadaEmUtc { get; private set; }
        public DateTime UltimaRenovacaoUtc { get; private set; }
        public DateTime ExpiraEmUtc { get; private set; }
        public DateTime? RevogadaEmUtc { get; private set; }
        public string? MotivoRevogacao { get; private set; }
        public Guid? SubstituidaPorId { get; private set; }

        public ApplicationUser Usuario { get; private set; }

        // Construtor reservado para o Entity Framework.
        protected SessaoRefresh()
        {
            UsuarioId = string.Empty;
            TokenHash = string.Empty;
            Usuario = null!;
        }

        public SessaoRefresh(
            string usuarioId,
            string tokenHash,
            TipoSessaoAutenticacaoEnum tipo,
            DateTime criadaEmUtc,
            DateTime expiraEmUtc)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentException(
                    "O usuário da sessão é obrigatório.",
                    nameof(usuarioId));

            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new ArgumentException(
                    "O hash do refresh token é obrigatório.",
                    nameof(tokenHash));

            if (expiraEmUtc <= criadaEmUtc)
                throw new ArgumentException(
                    "A expiração da sessão deve ser posterior à sua criação.",
                    nameof(expiraEmUtc));

            Id = Guid.NewGuid();
            UsuarioId = usuarioId;
            TokenHash = tokenHash;
            Tipo = tipo;
            CriadaEmUtc = criadaEmUtc;
            UltimaRenovacaoUtc = criadaEmUtc;
            ExpiraEmUtc = expiraEmUtc;
            Usuario = null!;
        }

        // Revoga a sessão e impede novas renovações.
        public void Revogar(
            DateTime revogadaEmUtc,
            string motivo)
        {
            if (RevogadaEmUtc.HasValue)
                return;

            RevogadaEmUtc = revogadaEmUtc;
            MotivoRevogacao = motivo;
        }

        // Marca a sessão atual como utilizada e aponta para a nova sessão
        // criada durante a rotação do refresh token.
        public void MarcarSubstituida(
            Guid sessaoSubstitutaId,
            DateTime renovadaEmUtc)
        {
            UltimaRenovacaoUtc = renovadaEmUtc;
            SubstituidaPorId = sessaoSubstitutaId;

            Revogar(
                renovadaEmUtc,
                "Refresh token rotacionado.");
        }
    }
}