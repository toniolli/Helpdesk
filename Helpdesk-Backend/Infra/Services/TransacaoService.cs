using Application.Interfaces;
using Infra.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infra.Services
{
    // Controla commit e rollback das operações que utilizam
    // o mesmo AppDbContext e seus respectivos repositórios.
    public class TransacaoService
        : ITransacaoService
    {
        private readonly AppDbContext _context;

        public TransacaoService(
            AppDbContext context)
        {
            _context = context;
        }

        // Executa a operação em uma nova transação ou participa
        // da transação que já estiver ativa no AppDbContext.
        public async Task ExecutarAsync(Func<Task> operacao)
        {
            if (_context.Database.CurrentTransaction is not null)
            {
                await operacao();
                return;
            }

            await using IDbContextTransaction transacao = await _context.Database.BeginTransactionAsync();

            try
            {
                await operacao();
                await transacao.CommitAsync();
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }
    }
}