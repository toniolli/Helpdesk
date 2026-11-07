using Domain.Validation;

namespace Domain.Entities
{
    // Representa os metadados de um arquivo anexado a um comentário.
    // O conteúdo físico fica no serviço de armazenamento; a entidade mantém apenas sua referência.
    public class Anexo
    {
        public int Id_Anexo { get; private set; }
        public int ComentarioId { get; private set; }
        public string NomeArquivo { get; private set; }
        public string NomeOriginal { get; private set; }
        public string ContentType { get; private set; }
        public long Tamanho { get; private set; }
        public string CaminhoArquivo { get; private set; }
        public DateTime DataUpload { get; private set; }
        public Comentario? Comentario { get; private set; }

        // Construtor exclusivo do Entity Framework.
        protected Anexo()
        {
        }

        public Anexo(
            int comentarioId,
            string nomeArquivo,
            string nomeOriginal,
            string contentType,
            long tamanho,
            string caminhoArquivo)
        {
            ValidateDomain(
                comentarioId,
                nomeArquivo,
                nomeOriginal,
                contentType,
                tamanho,
                caminhoArquivo);

            ComentarioId = comentarioId;
            NomeArquivo = nomeArquivo;
            NomeOriginal = nomeOriginal;
            ContentType = contentType;
            Tamanho = tamanho;
            CaminhoArquivo = caminhoArquivo;
            DataUpload = DateTime.Now;
        }

        // Garante que somente referências de arquivos válidas sejam criadas no domínio.
        private void ValidateDomain(
            int comentarioId,
            string nomeArquivo,
            string nomeOriginal,
            string contentType,
            long tamanho,
            string caminhoArquivo)
        {
            DomainExceptionValidation.when(
                comentarioId <= 0,
                "Comentário inválido");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(nomeArquivo),
                "Nome do arquivo é obrigatório");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(nomeOriginal),
                "Nome original é obrigatório");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(contentType),
                "Tipo do arquivo é obrigatório");

            DomainExceptionValidation.when(
                tamanho <= 0,
                "Tamanho do arquivo é inválido");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(caminhoArquivo),
                "Caminho do arquivo é obrigatório");
        }
    }
}
