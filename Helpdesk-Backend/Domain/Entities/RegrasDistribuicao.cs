using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Define quando, para quem e por quais categorias os chamados devem ser
    // distribuídos.
    public class RegraDistribuicao
    {
        public int Id_Regras { get; private set; }
        public string Nome { get; private set; }
        public TipoRegraEnum TipoRegra { get; private set; }
        public TipoDestinoEnum TipoDestino { get; private set; }
        public bool Ativo { get; private set; }
        public bool PeriodoIndefinido { get; private set; }
        public DateTime? DataInicial { get; private set; }
        public DateTime? DataFinal { get; private set; }
        public string? UsuarioDestinoId { get; private set; }
        public int? PerfilDestinoId { get; private set; }

        public ICollection<CriterioDistribuicao> Criterios { get; private set; }
            = new List<CriterioDistribuicao>();

        // Construtor reservado para materialização pelo Entity Framework.
        protected RegraDistribuicao()
        {
        }

        public RegraDistribuicao(
            string nome,
            TipoRegraEnum tipoRegra,
            TipoDestinoEnum tipoDestino,
            bool periodoIndefinido,
            DateTime? dataInicial,
            DateTime? dataFinal,
            string? usuarioDestinoId,
            int? perfilDestinoId)
        {
            ValidateDomain(
                nome,
                tipoRegra,
                tipoDestino,
                periodoIndefinido,
                dataInicial,
                dataFinal,
                usuarioDestinoId,
                perfilDestinoId);

            Nome = nome;
            TipoRegra = tipoRegra;
            TipoDestino = tipoDestino;
            PeriodoIndefinido = periodoIndefinido;
            DataInicial = dataInicial;
            DataFinal = dataFinal;
            UsuarioDestinoId = usuarioDestinoId;
            PerfilDestinoId = perfilDestinoId;

            Ativo = true;
        }

        // Garante os dados obrigatórios do período e do tipo de destino.
        private void ValidateDomain(
            string nome,
            TipoRegraEnum tipoRegra,
            TipoDestinoEnum tipoDestino,
            bool periodoIndefinido,
            DateTime? dataInicial,
            DateTime? dataFinal,
            string? usuarioDestinoId,
            int? perfilDestinoId)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(nome),
                "Nome da regra é obrigatório.");

            if (!periodoIndefinido)
            {
                DomainExceptionValidation.when(
                    !dataInicial.HasValue,
                    "Data inicial é obrigatória.");

                DomainExceptionValidation.when(
                    !dataFinal.HasValue,
                    "Data final é obrigatória.");

                DomainExceptionValidation.when(
                    dataFinal < dataInicial,
                    "Data final deve ser maior ou igual à data inicial.");
            }

            if (tipoDestino == TipoDestinoEnum.Usuario)
            {
                DomainExceptionValidation.when(
                    string.IsNullOrWhiteSpace(usuarioDestinoId),
                    "Usuário destino é obrigatório.");
            }

            if (tipoDestino == TipoDestinoEnum.Perfil)
            {
                DomainExceptionValidation.when(
                    !perfilDestinoId.HasValue ||
                    perfilDestinoId <= 0,
                    "Perfil destino é obrigatório.");
            }
        }

        // DADOS DA REGRA

        public void Atualizar(
            string nome,
            TipoRegraEnum tipoRegra,
            TipoDestinoEnum tipoDestino,
            bool ativo,
            bool periodoIndefinido,
            DateTime? dataInicial,
            DateTime? dataFinal,
            string? usuarioDestinoId,
            int? perfilDestinoId)
        {
            ValidateDomain(
                nome,
                tipoRegra,
                tipoDestino,
                periodoIndefinido,
                dataInicial,
                dataFinal,
                usuarioDestinoId,
                perfilDestinoId);

            Nome = nome;
            TipoRegra = tipoRegra;
            TipoDestino = tipoDestino;
            Ativo = ativo;
            PeriodoIndefinido = periodoIndefinido;
            DataInicial = dataInicial;
            DataFinal = dataFinal;
            UsuarioDestinoId = usuarioDestinoId;
            PerfilDestinoId = perfilDestinoId;
        }

        public void Ativar()
        {
            Ativo = true;
        }

        public void Desativar()
        {
            Ativo = false;
        }

        public void AlterarNome(string nome)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(nome),
                "Nome da regra é obrigatório.");

            Nome = nome;
        }

        // PERÍODO E DESTINO

        public void DefinirPeriodo(
            DateTime dataInicial,
            DateTime dataFinal)
        {
            DomainExceptionValidation.when(
                dataFinal < dataInicial,
                "Data final deve ser maior ou igual à data inicial.");

            PeriodoIndefinido = false;
            DataInicial = dataInicial;
            DataFinal = dataFinal;
        }

        public void DefinirPeriodoIndefinido()
        {
            PeriodoIndefinido = true;
            DataInicial = null;
            DataFinal = null;
        }

        public void AlterarDestinoUsuario(string usuarioDestinoId)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(usuarioDestinoId),
                "Usuário destino é obrigatório.");

            TipoDestino = TipoDestinoEnum.Usuario;
            UsuarioDestinoId = usuarioDestinoId;
            PerfilDestinoId = null;
        }

        // Retorna se a regra está dentro do período configurado.
        public bool EstaVigente()
        {
            if (PeriodoIndefinido)
                return true;

            if (!DataInicial.HasValue || !DataFinal.HasValue)
                return false;

            var agora = DateTime.Now;

            return agora >= DataInicial.Value &&
                   agora <= DataFinal.Value;
        }

        // Uma regra automática precisa estar ativa e vigente.
        public bool EhAutomatica()
        {
            return Ativo &&
                   TipoRegra == TipoRegraEnum.Automatico &&
                   EstaVigente();
        }

        // CRITÉRIOS

        // Impede que a mesma categoria seja adicionada duas vezes à regra.
        public void AdicionarCriterio(CriterioDistribuicao criterio)
        {
            DomainExceptionValidation.when(
                criterio == null,
                "Critério é obrigatório.");

            DomainExceptionValidation.when(
                Criterios.Any(c =>
                    c.CategoriaAtendimentoId ==
                    criterio.CategoriaAtendimentoId),
                "Esta categoria já foi adicionada à regra.");

            Criterios.Add(criterio);
        }

        public void RemoverCriterio(CriterioDistribuicao criterio)
        {
            DomainExceptionValidation.when(
                criterio == null,
                "Critério é obrigatório.");

            Criterios.Remove(criterio);
        }
    }
}
