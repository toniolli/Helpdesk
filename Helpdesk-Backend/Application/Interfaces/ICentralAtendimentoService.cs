using Application.Dto.CentralAtendimento;

namespace Application.Interfaces
{
    public interface ICentralAtendimentoService
    {
        // Monta uma fotografia completa com os dados atuais da Central.
        // O CancellationToken permite interromper o processamento
        // quando a requisição for cancelada ou a API estiver encerrando.
        Task<CentralAtendimentoResponseDto> ObterFotografiaAsync(CancellationToken cancellationToken = default);


    }
}