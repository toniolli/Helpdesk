namespace Application.DTOs.Anexo
{
    public class AnexoResponseDto
    {
        public int Id_Anexo { get; set; }
        public int ComentarioId { get; set; }
        public string NomeArquivo { get; set; }
        public string NomeOriginal { get; set; }
        public string ContentType { get; set; }
        public long Tamanho { get; set; }
        public string CaminhoArquivo { get; set; }
        public DateTime DataUpload { get; set; }
    }
}