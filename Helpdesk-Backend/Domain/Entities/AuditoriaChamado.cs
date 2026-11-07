using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Representa um registro histórico de uma ação realizada em um chamado.
    // Os dados são armazenados como uma fotografia do momento do evento.
    public class AuditoriaChamado
    {
        public int Id_AuditoriaChamado { get; private set; }
        public int ChamadoId { get; private set; }
        public string NumeroChamado { get; private set; } = null!;
        public EventoAuditoriaChamadoEnum Evento { get; private set; }
        public DateTime DataEvento { get; private set; }
        public string? UsuarioId { get; private set; }
        public string UsuarioNome { get; private set; } = null!;
        public string? PerfilUsuario { get; private set; }
        public StatusChamadoEnum? StatusAnterior { get; private set; }
        public StatusChamadoEnum? StatusNovo { get; private set; }
        public string Descricao { get; private set; } = null!;
        public string? DetalhesAnteriores { get; private set; }
        public string? DetalhesNovos { get; private set; }
        public Chamado Chamado { get; private set; } = null!;

        // Construtor reservado para materialização pelo Entity Framework.
        protected AuditoriaChamado()
        {
        }

        // A criação é controlada pelo chamado, mantendo a auditoria vinculada
        // ao agregado que originou o evento.
        internal AuditoriaChamado(
            Chamado chamado,
            string numeroChamado,
            EventoAuditoriaChamadoEnum evento,
            string? usuarioId,
            string usuarioNome,
            string? perfilUsuario,
            StatusChamadoEnum? statusAnterior,
            StatusChamadoEnum? statusNovo,
            string descricao,
            string? detalhesAnteriores,
            string? detalhesNovos)
        {
            Validate(
                chamado,
                numeroChamado,
                evento,
                usuarioNome,
                descricao);

            Chamado = chamado;
            ChamadoId = chamado.Id_Chamado;
            NumeroChamado = numeroChamado.Trim();
            Evento = evento;
            DataEvento = DateTime.UtcNow;
            UsuarioId = usuarioId;
            UsuarioNome = usuarioNome.Trim();
            PerfilUsuario = string.IsNullOrWhiteSpace(perfilUsuario)
                ? null
                : perfilUsuario.Trim();
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            Descricao = descricao.Trim();
            DetalhesAnteriores = detalhesAnteriores;
            DetalhesNovos = detalhesNovos;
        }

        // Garante que todo registro de auditoria seja criado com as
        // informações mínimas necessárias para preservar seu significado.
        private static void Validate(
            Chamado chamado,
            string numeroChamado,
            EventoAuditoriaChamadoEnum evento,
            string usuarioNome,
            string descricao)
        {
            DomainExceptionValidation.when(
                chamado == null,
                "O chamado da auditoria é obrigatório.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(numeroChamado),
                "O número do chamado da auditoria é obrigatório.");

            DomainExceptionValidation.when(
                !System.Enum.IsDefined(
                    typeof(EventoAuditoriaChamadoEnum),
                    evento),
                "O evento da auditoria do chamado é inválido.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(usuarioNome),
                "O nome do usuário da auditoria é obrigatório.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(descricao),
                "A descrição da auditoria do chamado é obrigatória.");
        }
    }
}
