using Domain.Validation;

namespace Domain.Entities
{
    // Representa uma permissão que pode ser vinculada aos perfis do sistema.
    public class Permissao
    {
        public int Id_Permissao { get; private set; }
        public string Nome { get; private set; }
        public string? Descricao { get; private set; }
        public bool Ativo { get; private set; }
        public ICollection<PerfilPermissao> PerfisPermissoes { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected Permissao()
        {
        }

        public Permissao(
            string nome,
            string? descricao)
        {
            ValidateDomain(nome);

            Nome = nome;
            Descricao = descricao;
            Ativo = true;
        }

        // Garante que toda permissão possua um nome.
        private void ValidateDomain(string nome)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(nome),
                "Nome da permissão é obrigatório");
        }

        // DADOS DA PERMISSÃO

        public void Atualizar(
            string nome,
            string? descricao)
        {
            ValidateDomain(nome);

            Nome = nome;
            Descricao = descricao;
        }

        // ESTADO

        public void Ativar()
        {
            Ativo = true;
        }

        public void Desativar()
        {
            Ativo = false;
        }
    }
}
