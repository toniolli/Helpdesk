using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SalvarArquivo(IFormFile arquivo);
        Task RemoverArquivo(string caminhoArquivo);

        Task<(byte[] Arquivo, string ContentType, string Nome)> BaixarArquivo(string caminhoArquivo);


    }
}
