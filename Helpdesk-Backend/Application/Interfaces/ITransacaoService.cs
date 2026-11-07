namespace Application.Interfaces
{
    // Executa uma operação dentro de uma transação compartilhada
    // pelo AppDbContext e por todos os repositórios envolvidos.
    public interface ITransacaoService
    {
        Task ExecutarAsync(
            Func<Task> operacao);
    }
}