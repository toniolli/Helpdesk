using Domain.Validation;

namespace Domain.Entities
{
    // Representa um perfil de acesso e mantém o histórico versionado de seus
    // períodos de trabalho.
    public class Perfil
    {
        public int Id_Perfil { get; private set; }
        public string Nome { get; private set; }
        public string? Descricao { get; private set; }
        public bool Ativo { get; private set; }

        public ICollection<UsuarioPerfil> UsuariosPerfis { get; private set; }
            = new List<UsuarioPerfil>();

        public ICollection<PerfilPermissao> PerfisPermissoes { get; private set; }
            = new List<PerfilPermissao>();

        public ICollection<PeriodoTrabalhoPerfil> PeriodosTrabalho { get; private set; }
            = new List<PeriodoTrabalhoPerfil>();

        // Retorna somente a versão do período que ainda está vigente.
        public PeriodoTrabalhoPerfil? PeriodoTrabalhoVigente =>
            PeriodosTrabalho.SingleOrDefault(
                periodo => periodo.EstaVigente());

        // Construtor reservado para materialização pelo Entity Framework.
        protected Perfil()
        {
        }

        public Perfil(
            string nome,
            string? descricao)
        {
            ValidateDomain(nome);

            Nome = nome;
            Descricao = descricao;
            Ativo = true;
        }

        // Garante que todo perfil possua um nome.
        private void ValidateDomain(string nome)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(nome),
                "Nome do perfil é obrigatório");
        }

        // DADOS DO PERFIL

        public void Atualizar(
            string nome,
            string? descricao)
        {
            ValidateDomain(nome);

            Nome = nome;
            Descricao = descricao;
        }

        public void Ativar()
        {
            Ativo = true;
        }

        public void Desativar()
        {
            Ativo = false;
        }

        // PERÍODO DE TRABALHO

        // Preserva o período atual quando a configuração não mudou. Quando há
        // mudança, encerra a vigência anterior e adiciona uma nova versão.
        public void DefinirPeriodoTrabalho(
            PeriodoTrabalhoPerfil novoPeriodo,
            DateTime agora)
        {
            DomainExceptionValidation.when(
                novoPeriodo == null,
                "Período de trabalho é obrigatório.");

            var periodoVigente = PeriodoTrabalhoVigente;

            if (periodoVigente != null &&
                periodoVigente.PossuiMesmaConfiguracao(novoPeriodo!))
            {
                return;
            }

            periodoVigente?.EncerrarVigencia(agora);

            var proximaVersao = PeriodosTrabalho.Count == 0
                ? 1
                : PeriodosTrabalho.Max(periodo => periodo.Versao) + 1;

            novoPeriodo!.VincularAoPerfil(
                this,
                proximaVersao,
                agora);

            PeriodosTrabalho.Add(novoPeriodo);
        }

        // Encerra somente o período vigente do perfil. As versões antigas
        // permanecem preservadas para os chamados que já utilizam esse período.
        public void EncerrarPeriodoTrabalho(DateTime agora)
        {
            PeriodoTrabalhoVigente?.EncerrarVigencia(agora);
        }
    }
}
