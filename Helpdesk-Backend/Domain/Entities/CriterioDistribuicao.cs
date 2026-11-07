using Domain.Validation;

namespace Domain.Entities
{
    // Relaciona uma regra de distribuição a uma categoria de atendimento.
    public class CriterioDistribuicao
    {
        public int Id_Criterio { get; private set; }
        public int RegraDistribuicaoId { get; private set; }
        public int CategoriaAtendimentoId { get; private set; }
        public RegraDistribuicao? RegraDistribuicao { get; private set; }
        public CategoriaAtendimento? CategoriaAtendimento { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected CriterioDistribuicao()
        {
        }

        public CriterioDistribuicao(
            int regraDistribuicaoId,
            int categoriaAtendimentoId)
        {
            ValidateDomain(
                regraDistribuicaoId,
                categoriaAtendimentoId);

            RegraDistribuicaoId = regraDistribuicaoId;
            CategoriaAtendimentoId = categoriaAtendimentoId;
        }

        // Garante que o critério esteja associado a uma regra e uma categoria.
        private void ValidateDomain(
            int regraDistribuicaoId,
            int categoriaAtendimentoId)
        {
            DomainExceptionValidation.when(
                regraDistribuicaoId <= 0,
                "Regra de distribuição é obrigatória.");

            DomainExceptionValidation.when(
                categoriaAtendimentoId <= 0,
                "Categoria de atendimento é obrigatória.");
        }
    }
}
