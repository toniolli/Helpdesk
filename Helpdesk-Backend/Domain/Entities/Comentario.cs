using Domain.Validation;

namespace Domain.Entities
{
    // Representa uma mensagem adicionada ao histórico de um chamado
    // e mantém os anexos vinculados a essa interação.
    public class Comentario
    {
        public int Id_Comentario { get; private set; }
        public int ChamadoId { get; private set; }
        public string UsuarioId { get; private set; }
        public string? Mensagem { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public Chamado? Chamado { get; private set; }
        public ICollection<Anexo> Anexos { get; private set; }
            = new List<Anexo>();

        // Construtor reservado para materialização pelo Entity Framework.
        protected Comentario()
        {
        }

        public Comentario(
            int chamadoId,
            string usuarioId,
            string? mensagem)
        {
            ValidateDomain(
                chamadoId,
                usuarioId,
                mensagem);

            ChamadoId = chamadoId;
            UsuarioId = usuarioId;
            Mensagem = string.IsNullOrWhiteSpace(mensagem) ? null : mensagem;
            DataCriacao = DateTime.Now;
        }

        
    
        private void ValidateDomain(
            int chamadoId,
            string usuarioId,
            string? mensagem)
        {
            DomainExceptionValidation.when(
                chamadoId <= 0,
                "Chamado inválido");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(usuarioId),
                "Usuário é obrigatório");

            DomainExceptionValidation.when(
              mensagem?.Length > 4000,
              "A mensagem não pode exceder 4000 caracteres");
        }

        // Atualiza somente o conteúdo, preservando autor e data de criação.
        public void AtualizarMensagem(string mensagem)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(mensagem),
                "Mensagem é obrigatória");

            DomainExceptionValidation.when(
               mensagem.Length > 4000,
               "A mensagem não pode exceder 4000 caracteres");

            Mensagem = mensagem;
        }
    }
}
