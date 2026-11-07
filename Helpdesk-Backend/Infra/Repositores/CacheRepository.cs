using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infra.Repositores
{
    public class CacheRepository : ICacheRepository
    {
        private readonly IMemoryCache _cache;

        public CacheRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<T?> Obter<T>(string chave)
        {
            _cache.TryGetValue(chave, out T? valor);

            return Task.FromResult(valor);
        }

        public Task Salvar<T>(string chave,T valor,TimeSpan expiracao)
        {
            _cache.Set(chave, valor, expiracao);
            return Task.CompletedTask;
        }

        public Task Remover(string chave)
        {
            _cache.Remove(chave);
            return Task.CompletedTask;
        }
    }
}