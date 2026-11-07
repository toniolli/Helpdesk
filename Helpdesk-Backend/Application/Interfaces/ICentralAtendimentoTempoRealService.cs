using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICentralAtendimentoTempoRealService
    {
        Guid InstanciaId { get; }
        long GerarProximaVersao();
        bool PossuiConexoes { get; }
        void AdicionarConexao(string connectionId);
        void RemoverConexao(string connectionId);
        void SinalizarAtualizacao();
        ValueTask AguardarAtualizacaoAsync(CancellationToken cancellationToken = default);
        bool TentarConsumirAtualizacao();
    }
}
