using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infra.Services
{
    public class FileStorageService : IFileStorageService
    {
        private const long TamanhoMaximoArquivo = 10 * 1024 * 1024;
        private static readonly HashSet<string> ExtensoesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".pdf", ".txt", ".docx"
        };
        private readonly string _uploadPath;

        public FileStorageService()
        {
            _uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "uploads",
                "helpdesk");

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> SalvarArquivo(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                throw new ArgumentException("Arquivo inválido.");

            if (arquivo.Length > TamanhoMaximoArquivo)
                throw new ArgumentException("O arquivo não pode exceder 10 MB.");

            var extensao = Path.GetExtension(arquivo.FileName);

            if (!ExtensoesPermitidas.Contains(extensao))
                throw new ArgumentException("Tipo de arquivo não permitido.");

            var nomeArquivo =
                $"{Guid.NewGuid()}{extensao}";

            var caminhoCompleto =
                Path.Combine(_uploadPath, nomeArquivo);
            //Save in disc
            using (var stream = new FileStream(
                caminhoCompleto,
                FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }
            return nomeArquivo;
        }

        public async Task<(byte[] Arquivo, string ContentType, string Nome)> BaixarArquivo(string caminhoArquivo)
        {
            var caminhoCompleto = ObterCaminhoSeguro(caminhoArquivo);

            if (!File.Exists(caminhoCompleto))
                throw new FileNotFoundException("Arquivo não encontrado.");

            var bytes = await File.ReadAllBytesAsync(caminhoCompleto);

            var nome = Path.GetFileName(caminhoCompleto);

            var contentType = ObterContentType(nome);

            return (
                bytes,
                contentType,
                nome
            );
        }

        private string ObterContentType(string nomeArquivo)
        {
            var extensao = Path.GetExtension(nomeArquivo)
                .ToLowerInvariant();

            return extensao switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        public Task RemoverArquivo(string caminhoArquivo)
        {
            //if path is empty return nothing
            if (string.IsNullOrWhiteSpace(caminhoArquivo))
                return Task.CompletedTask;
            var caminhoCompleto = ObterCaminhoSeguro(caminhoArquivo);

            if (File.Exists(caminhoCompleto))
            {
                File.Delete(caminhoCompleto);
            }

            return Task.CompletedTask;
        }

        private string ObterCaminhoSeguro(string caminhoArquivo)
        {
            var nomeArquivo = Path.GetFileName(caminhoArquivo);
            var caminhoLegado = $"uploads/helpdesk/{nomeArquivo}";

            if (string.IsNullOrWhiteSpace(nomeArquivo) ||
                (!string.Equals(nomeArquivo, caminhoArquivo, StringComparison.Ordinal) &&
                 !string.Equals(caminhoLegado, caminhoArquivo.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Caminho de arquivo inválido.");
            }

            if (string.Equals(caminhoLegado, caminhoArquivo.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "helpdesk",
                    nomeArquivo);
            }

            return Path.Combine(_uploadPath, nomeArquivo);
        }
    }
}
