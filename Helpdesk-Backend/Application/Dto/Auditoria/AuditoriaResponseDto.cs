using Domain.Enum;

namespace Application.Dto.Auditoria
{
    public class AuditoriaResponseDto
    {
        public int Id_Auditoria { get; set; }
        public string? UsuarioId { get; set; }
        public string UsuarioNome { get; set; }
        public AcaoAuditoriaEnum Acao { get; set; }
        public OrigemAuditoriaEnum Origem { get; set; }
        public string Entidade { get; set; }
        public string RegistroId { get; set; }
        public string? ValoresAnteriores { get; set; }
        public string? ValoresAtuais { get; set; }
        public string? EnderecoIp { get; set; }
        public string? MetodoHttp { get; set; }
        public string? Rota { get; set; }
        public string TraceId { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}