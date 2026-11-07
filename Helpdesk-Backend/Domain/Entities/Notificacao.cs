using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Representa um aviso destinado a um usuário e mantém seu estado de
    // leitura e decisão dentro dos fluxos do chamado.
    public class Notificacao
    {
        public int Id_Notificacao { get; private set; }
        public string UsuarioId { get; private set; }
        public string? UsuarioOrigemId { get; private set; }
        public int ChamadoId { get; private set; }
        public TipoNotificacaoEnum Tipo { get; private set; }
        public string Titulo { get; private set; }
        public string Mensagem { get; private set; }
        public bool Lida { get; private set; }
        public StatusNotificacaoEnum Status { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataLida { get; private set; }

        public Chamado Chamado { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected Notificacao()
        {
        }

        public Notificacao(
            string usuarioId,
            string usuarioOrigemId,
            int chamadoId,
            TipoNotificacaoEnum tipo,
            string titulo,
            string mensagem,
            StatusNotificacaoEnum status)
        {
            ValidateDomain(
                usuarioId,
                chamadoId,
                titulo,
                mensagem);

            UsuarioId = usuarioId;
            UsuarioOrigemId = usuarioOrigemId;
            ChamadoId = chamadoId;
            Tipo = tipo;
            Titulo = titulo;
            Mensagem = mensagem;
            Status = status;
            Lida = false;
            DataCriacao = DateTime.Now;
            DataLida = null;
        }

        // Garante que a notificação possua destinatário, chamado e conteúdo.
        private void ValidateDomain(
            string usuarioId,
            int chamadoId,
            string titulo,
            string mensagem)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(usuarioId),
                "Usuario e obirgatorio!");

            DomainExceptionValidation.when(
                chamadoId <= 0,
                "Chamado invalido");

            DomainExceptionValidation.when(
                String.IsNullOrWhiteSpace(titulo),
                "Titulos é obrigatorio!");

            DomainExceptionValidation.when(
                String.IsNullOrWhiteSpace(mensagem),
                "Mensagem é obrigatorio!");
        }

        // Registra a leitura e o instante em que ela ocorreu.
        public void MarcarComoLida()
        {
            Lida = true;
            DataLida = DateTime.Now;
        }

        // Confirma a ação proposta pela notificação.
        public void Aceitar()
        {
            Status = StatusNotificacaoEnum.Aceita;
        }

        // Recusa a ação proposta pela notificação.
        public void Recusar()
        {
            Status = StatusNotificacaoEnum.Recusada;
        }

        // Indica que a notificação perdeu sua validade.
        public void Expirar()
        {
            Status = StatusNotificacaoEnum.Expirada;
        }

        // Devolve a notificação ao estado de espera.
        public void Pendente()
        {
            Status = StatusNotificacaoEnum.Pendente;
        }
    }
}
