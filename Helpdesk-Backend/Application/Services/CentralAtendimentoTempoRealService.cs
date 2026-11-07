using Application.Interfaces;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Threading.Channels;

namespace Application.Services
{
    public sealed class CentralAtendimentoTempoRealService : ICentralAtendimentoTempoRealService
    {
        private readonly ConcurrentDictionary<string, byte> _conexoes = new();
        private readonly Guid _instanciaId = Guid.NewGuid();
        private long _versao;


        private readonly Channel<bool> _canalAtualizacoes = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
        {
             SingleReader = true,
             SingleWriter = false,
             FullMode = BoundedChannelFullMode.DropWrite
        });

        //Guarda os IDs das conexões SignalR que estão usando a Central.
        public bool PossuiConexoes => !_conexoes.IsEmpty;

        public void AdicionarConexao(string connectionId)
        {
            _conexoes.TryAdd(connectionId,0);
        }

        public void RemoverConexao(string connectionId)
        {
            _conexoes.TryRemove(connectionId, out _);
        }

        public void SinalizarAtualizacao()
        {
            _canalAtualizacoes.Writer.TryWrite(true);
        }

        public async ValueTask AguardarAtualizacaoAsync(CancellationToken cancellationToken = default)
        {
            await _canalAtualizacoes.Reader.ReadAsync(cancellationToken);
        }

        public bool TentarConsumirAtualizacao()
        {
            return _canalAtualizacoes.Reader.TryRead(out _);
        }

        public Guid InstanciaId => _instanciaId;

        public long GerarProximaVersao()
        {
            return Interlocked.Increment(ref _versao);
        }
    }
}