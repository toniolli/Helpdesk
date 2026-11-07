using Application.Interfaces;
using Domain.Interfaces;

namespace Application.Services
{
    // Centraliza o acesso ao cache da aplicação e evita consultas repetidas
    // ao banco enquanto o valor armazenado permanecer válido.
    public class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;

        public CacheService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }

        // Retorna o valor armazenado no cache ou executa a consulta informada,
        // salvando o resultado pelo período de expiração recebido.
        public async Task<T> ObterOuCriar<T>(
            string chave,
            Func<Task<T>> buscarNoBanco,
            TimeSpan expiracao)
        {
            var valorCache = await _cacheRepository.Obter<T>(chave);

            if (valorCache is not null)
            {
                return valorCache;
            }

            var valorBanco = await buscarNoBanco();

            await _cacheRepository.Salvar(
                chave,
                valorBanco,
                expiracao);

            return valorBanco;
        }

        // Invalida o valor associado à chave para que a próxima consulta
        // obtenha os dados atualizados na fonte original.
        public async Task Remover(string chave)
        {
            await _cacheRepository.Remover(chave);
        }
    }
}
