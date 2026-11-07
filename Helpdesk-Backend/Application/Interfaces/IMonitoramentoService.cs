using Application.ViewModel;

namespace Application.Interfaces
{
    public interface IMonitoramentoService
    {
        // Uso interno pelo middleware
        Task RegistrarRequisicao(string metodoHttp,string endpoint,
            int statusCode, double tempoRespostaMs, string? traceId);
        // Uso interno pelo capturador de logs
        Task RegistrarErro(string origem, string mensagem, Exception? exception,string? metodoHttp, 
            string? endpoint,int? statusCode, string? traceId);
        // Consulta administrativa
        Task<PainelErrosViewModel> ObterPainel();
        // Uso interno pelo processo de retenção
        Task<int> RemoverExpirados();
    }
}