using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Registra alterações realizadas nas entidades do sistema.
    // Os valores anteriores e atuais preservam o estado observado no evento.
    public class Auditoria
    {
        public int Id_Auditoria { get; private set; }
        public string? UsuarioId { get; private set; }
        public string UsuarioNome { get; private set; }
        public AcaoAuditoriaEnum Acao { get; private set; }
        public OrigemAuditoriaEnum Origem { get; private set; }
        public string Entidade { get; private set; }
        public string RegistroId { get; private set; }
        public string? ValoresAnteriores { get; private set; }
        public string? ValoresAtuais { get; private set; }
        public string? EnderecoIp { get; private set; }
        public string? MetodoHttp { get; private set; }
        public string? Rota { get; private set; }
        public string TraceId { get; private set; }
        public DateTime DataCriacao { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected Auditoria()
        {
        }

        public Auditoria(
            string? usuarioId,
            string usuarioNome,
            AcaoAuditoriaEnum acao,
            OrigemAuditoriaEnum origem,
            string entidade,
            string registroId,
            string? valoresAnteriores,
            string? valoresAtuais,
            string? enderecoIp,
            string? metodoHttp,
            string? rota,
            string traceId)
        {
            Validate(
                usuarioNome,
                entidade,
                registroId,
                traceId);

            UsuarioId = usuarioId;
            UsuarioNome = usuarioNome.Trim();
            Acao = acao;
            Origem = origem;
            Entidade = entidade.Trim();
            RegistroId = registroId.Trim();
            ValoresAnteriores = valoresAnteriores;
            ValoresAtuais = valoresAtuais;
            EnderecoIp = enderecoIp;
            MetodoHttp = metodoHttp;
            Rota = rota;
            TraceId = traceId.Trim();
            DataCriacao = DateTime.UtcNow;
        }

        // Garante que o registro contenha as informações mínimas necessárias
        // para identificar a alteração e correlacioná-la à requisição.
        private static void Validate(
            string usuarioNome,
            string entidade,
            string registroId,
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
                string.IsNullOrWhiteSpace(traceId),
                "O TraceId da auditoria é obrigatório.");
        }
    }
}
