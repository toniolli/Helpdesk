using Domain.Validation;

namespace Domain.Entities
{
    // Representa uma categoria utilizada para classificar o atendimento
    // e definir a prioridade inicial dos chamados associados a ela.
    public class CategoriaAtendimento
    {
        public int Id_CategoriaAtendimento { get; private set; }
        public string Nome { get; private set; }
        public string? Descricao { get; private set; }
        public PrioridadeEnum Prioridade { get; private set; }
        public bool Ativo { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected CategoriaAtendimento()
        {
        }

        public CategoriaAtendimento(
            string nome,
            string? descricao,
            PrioridadeEnum prioridade)
        {
            ValidateDomain(nome);

            Nome = nome;
            Descricao = descricao;
            Prioridade = prioridade;
            Ativo = true;
        }

        // Atualiza os dados editáveis sem alterar a situação da categoria.
        public void Atualizar(
            string nome,
            string? descricao,
            PrioridadeEnum prioridade)
        {
            ValidateDomain(nome);

            Nome = nome;
            Descricao = descricao;
            Prioridade = prioridade;
        }

        public void Ativar()
        {
            Ativo = true;
        }

        public void Desativar()
        {
            Ativo = false;
        }

        // Garante que a categoria possua a informação mínima obrigatória.
        private static void ValidateDomain(string nome)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(nome),
                "Nome da categoria é obrigatório");
        }
    }
}
