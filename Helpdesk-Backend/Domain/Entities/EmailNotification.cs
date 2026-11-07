using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Representa o envio de um e-mail relacionado a um chamado e mantém o
    // estado necessário para controlar tentativas, falhas e cancelamentos.
    public class EmailNotification
    {
        public int Id_EmailNotfication { get; private set; }
        public int ChamadoId { get; private set; }
        public TipoNotificacaoEmailEnum Tipo { get; private set; }
        public string Destinatario { get; set; }
        public string Assunto { get; set; }
        public StatusNotificacaoEmailEnum Status { get; private set; }
        public int QuantidadeTentativas { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataEnvio { get; private set; }
        public string? MensagemErro { get; private set; }
        public Chamado Chamado { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected EmailNotification()
        {
        }

        public EmailNotification(
            int chamadoID,
            TipoNotificacaoEmailEnum tipo,
            string destinatario,
            string assunto)
        {
            ValidateDomain(
                chamadoID,
                destinatario,
                assunto);

            ChamadoId = chamadoID;
            Tipo = tipo;
            Destinatario = destinatario;
            Assunto = assunto;
            QuantidadeTentativas = 0;
            Status = StatusNotificacaoEmailEnum.AguardoEnvio;
            DataCriacao = DateTime.Now;
        }

        // Garante que a notificação possua os dados mínimos para ser enviada.
        private void ValidateDomain(
            int chamadoID,
            string destinatario,
            string assunto)
        {
            DomainExceptionValidation.when(
                chamadoID <= 0,
                "Chamado e obrigatorio!");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(destinatario),
                "Destinatario não pode ser vazio!");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(assunto),
                "Assunto e obrigatorio!");
        }

        // Confirma o envio e remove uma eventual mensagem de erro anterior.
        public void MarcarComoEnviado()
        {
            Status = StatusNotificacaoEmailEnum.Enviado;
            DataEnvio = DateTime.Now;
            MensagemErro = null;
        }

        // Registra a falha e contabiliza uma nova tentativa de envio.
        public void RegistrarFalha(string mensagemErro)
        {
            Status = StatusNotificacaoEmailEnum.Falhou;
            QuantidadeTentativas++;
            MensagemErro = mensagemErro;
        }

        // Impede que a notificação continue disponível para envio.
        public void Cancelar()
        {
            Status = StatusNotificacaoEmailEnum.Cancelado;
        }
    }
}
