using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Registra ações relacionadas à identidade e ao acesso dos usuários.
    // Os valores anteriores e atuais preservam o estado observado no evento.
    public class AuditoriaIdentity
    {
        public int Id_AuditoriaIdentity { get; private set; }
        public string? UsuarioId { get; private set; }
        public string UsuarioNome { get; private set; }
        public EventoAuditoriaIdentityEnum Evento { get; private set; }
        public OrigemAuditoriaEnum Origem { get; private set; }
        public string Entidade { get; private set; }
        public string RegistroId { get; private set; }
        public bool Sucesso { get; private set; }
        public string Motivo { get; private set; }
        public string? ValoresAnteriores { get; private set; }
        public string? ValoresAtuais { get; private set; }
        public string? EnderecoIp { get; private set; }
        public string? MetodoHttp { get; private set; }
        public string? Rota { get; private set; }
        public string? UserAgent { get; private set; }
        public string TraceId { get; private set; }
        public DateTime DataCriacao { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected AuditoriaIdentity()
        {
        }

        public AuditoriaIdentity(
            string? usuarioId,
            string usuarioNome,
            EventoAuditoriaIdentityEnum evento,
            OrigemAuditoriaEnum origem,
            string entidade,
            string registroId,
            bool sucesso,
            string motivo,
            string? valoresAnteriores,
            string? valoresAtuais,
            string? enderecoIp,
            string? metodoHttp,
            string? rota,
            string? userAgent,
            string traceId)
        {
            Validate(
                usuarioNome,
                entidade,
                registroId,
                motivo,
                traceId);

            UsuarioId = usuarioId;
            UsuarioNome = usuarioNome.Trim();
            Evento = evento;
            Origem = origem;
            Entidade = entidade.Trim();
            RegistroId = registroId.Trim();
            Sucesso = sucesso;
            Motivo = motivo.Trim();
            ValoresAnteriores = valoresAnteriores;
            ValoresAtuais = valoresAtuais;
            EnderecoIp = enderecoIp;
            MetodoHttp = metodoHttp;
            Rota = rota;
            UserAgent = userAgent;
            TraceId = traceId.Trim();
            DataCriacao = DateTime.UtcNow;
        }

        // Garante que o registro contenha as informações mínimas necessárias
        // para identificar a ação auditada e correlacioná-la à requisição.
        private static void Validate(
            string usuarioNome,
            string entidade,
            string registroId,
            string motivo,
            string traceId)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(usuarioNome),
                "O nome do usuário da auditoria é obrigatório.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(entidade),
                "A entidade da auditoria é obrigatória.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(registroId),
                "O identificador do registro auditado é obrigatório.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(motivo),
                "O motivo da auditoria é obrigatório.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(traceId),
                "O TraceId da auditoria é obrigatório.");
        }
    }
}
