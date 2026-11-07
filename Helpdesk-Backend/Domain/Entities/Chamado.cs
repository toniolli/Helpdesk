using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Agregado principal do atendimento. Centraliza as transições de status,
    // pausas do SLA, encerramento, auditoria e cálculos de tempo do chamado.
    public class Chamado
    {
        public int Id_Chamado { get; private set; }
        public string? Numero { get; private set; }
        public string? SolicitanteId { get; private set; }
        public int CategoriaAtendimentoId { get; private set; }
        public string EquipamentoAfetado { get; private set; }
        public string Descricao { get; private set; }
        public PrioridadeEnum Prioridade { get; private set; }
        public StatusChamadoEnum Status { get; private set; }
        public string? TecnicoResponsavelId { get; private set; }
        public TipoAtendimentoEnum? TipoAtendimento { get; private set; }
        public TipoEncerramentoEnum? TipoEncerramento { get; private set; }
        public string? Solucao { get; private set; }
        public int Version { get; private set; } = 1;
        public DateTime DataAbertura { get; private set; }
        public DateTime? DataPrimeiroAtendimento { get; private set; }
        public DateTime? DataInicioPausaSla { get; private set; }
        public double TempoPausadoHoras { get; private set; }
        public DateTime? DataFechamento { get; private set; }
        public double TempoPausadoEquipamentoHoras { get; private set; }
        public int? PeriodoTrabalhoPerfilId { get; private set; }
        public double TempoPausadoSlaRetornoHoras { get; private set; }
        public double TempoPausadoSlaEquipamentoHoras { get; private set; }

        public ICollection<Comentario> Comentarios { get; private set; }
            = new List<Comentario>();

        public ICollection<Notificacao> Notificacoes { get; private set; }
            = new List<Notificacao>();

        public ICollection<AuditoriaChamado> Auditorias { get; private set; }
            = new List<AuditoriaChamado>();

        public CategoriaAtendimento? CategoriaAtendimento { get; private set; }
        public PeriodoTrabalhoPerfil? PeriodoTrabalhoPerfil { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected Chamado()
        {
        }

        public Chamado(
            string solicitanteId,
            int categoriaAtendimentoId,
            string equipamentoAfetado,
            string descricao,
            PrioridadeEnum prioridade)
        {
            ValidateDomain(
                solicitanteId,
                equipamentoAfetado,
                descricao);

            SolicitanteId = solicitanteId;
            CategoriaAtendimentoId = categoriaAtendimentoId;
            EquipamentoAfetado = equipamentoAfetado;
            Descricao = descricao;
            Prioridade = prioridade;
            Status = StatusChamadoEnum.Aberto;
            DataAbertura = DateTime.Now;
        }

        // Garante os dados mínimos necessários para a abertura do chamado.
        private void ValidateDomain(
            string solicitanteId,
            string equipamentoAfetado,
            string descricao)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(solicitanteId),
                "Solicitante é obrigatório");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(equipamentoAfetado),
                "Equipamento afetado é obrigatório");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(descricao),
                "Descrição do chamado é obrigatória");
        }

        // FLUXO DE ATENDIMENTO

        // Vincula o técnico e registra o primeiro atendimento somente uma vez.
        public void AssumirAtendimento(string tecnicoId)
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.Aberto &&
                Status != StatusChamadoEnum.Distribuido,
                "Somente chamados abertos podem ser assumidos");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(tecnicoId),
                "Técnico responsável é obrigatório");

            DomainExceptionValidation.when(
                !string.IsNullOrWhiteSpace(TecnicoResponsavelId) &&
                TecnicoResponsavelId != tecnicoId,
                "Este chamado já foi distribuído para outro técnico.");

            TecnicoResponsavelId = tecnicoId;
            Status = StatusChamadoEnum.EmAtendimento;

            if (DataPrimeiroAtendimento == null)
            {
                DataPrimeiroAtendimento = DateTime.Now;
            }
        }

        // Inicia a pausa enquanto o atendimento aguarda o solicitante.
        public void AguardarRetorno()
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.EmAtendimento &&
                Status != StatusChamadoEnum.AguardandoRetorno,
                "Somente chamado em atendimento ou aguardando retorno podem executar esta ação");

            Status = StatusChamadoEnum.AguardandoRetorno;

            if (DataInicioPausaSla == null)
            {
                DataInicioPausaSla = DateTime.Now;
            }
        }

        // Encerra uma eventual pausa de retorno e inicia a pausa de equipamento.
        public void AguardarEquipamento()
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.EmAtendimento &&
                Status != StatusChamadoEnum.AguardandoRetorno,
                "Somente chamados em atendimento ou aguardando retorno podem aguardar equipamento.");

            var agora = DateTime.Now;

            if (Status == StatusChamadoEnum.AguardandoRetorno &&
                DataInicioPausaSla.HasValue)
            {
                TempoPausadoHoras +=
                    (agora - DataInicioPausaSla.Value).TotalHours;

                TempoPausadoSlaRetornoHoras +=
                    CalcularHorasPausaSla(
                        DataInicioPausaSla.Value,
                        agora);
            }

            Status = StatusChamadoEnum.AguardandoEquipamento;
            DataInicioPausaSla = agora;
        }

        // Encerra a pausa de equipamento e devolve o chamado ao atendimento.
        public void ReceberEquipamento()
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.AguardandoEquipamento,
                "O chamado não está aguardando equipamento.");

            var agora = DateTime.Now;

            if (DataInicioPausaSla.HasValue)
            {
                TempoPausadoEquipamentoHoras +=
                    (agora - DataInicioPausaSla.Value).TotalHours;

                TempoPausadoSlaEquipamentoHoras +=
                    CalcularHorasPausaSla(
                        DataInicioPausaSla.Value,
                        agora);

                DataInicioPausaSla = null;
            }

            Status = StatusChamadoEnum.EmAtendimento;
        }

        // Encerra a pausa de retorno e devolve o chamado ao atendimento.
        public void RetomarAtendimento()
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.AguardandoRetorno,
                "O chamado não está aguardando retorno.");

            if (DataInicioPausaSla.HasValue)
            {
                var agora = DateTime.Now;

                TempoPausadoHoras +=
                    (agora - DataInicioPausaSla.Value).TotalHours;

                TempoPausadoSlaRetornoHoras +=
                    CalcularHorasPausaSla(
                        DataInicioPausaSla.Value,
                        agora);

                DataInicioPausaSla = null;
            }

            Status = StatusChamadoEnum.EmAtendimento;
        }

        // DISTRIBUIÇÃO E RESPONSABILIDADE

        public void DistribuirParaUsuario(string tecnicoResponsavelId)
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.Aberto,
                "Somente chamados abertos podem ser distribuídos.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(tecnicoResponsavelId),
                "Técnico responsável é obrigatório.");

            DomainExceptionValidation.when(
                !string.IsNullOrWhiteSpace(TecnicoResponsavelId),
                "O chamado já possui técnico responsável.");

            TecnicoResponsavelId = tecnicoResponsavelId;
            Status = StatusChamadoEnum.Distribuido;
        }

        public void DistribuirParaPerfil()
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.Aberto,
                "Somente chamados abertos podem ser distribuídos.");

            DomainExceptionValidation.when(
                !string.IsNullOrWhiteSpace(TecnicoResponsavelId),
                "O chamado já possui técnico responsável.");

            Status = StatusChamadoEnum.Distribuido;
        }

        public void TransferirChamado(string tecnicoDestinoId)
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.EmAtendimento,
                "Somente chamados em atendimento podem ser transferidos");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(tecnicoDestinoId),
                "Técnico Destino é obrigatório");

            DomainExceptionValidation.when(
                TecnicoResponsavelId == tecnicoDestinoId,
                "O chamado já está atribuído para este técnico.");

            TecnicoResponsavelId = tecnicoDestinoId;
        }

        // ALTERAÇÃO DOS DADOS

        public void AlterarDescricao(string descricao)
        {
            DomainExceptionValidation.when(
                Status == StatusChamadoEnum.Fechado ||
                Status == StatusChamadoEnum.Cancelado,
                "Não é possível alterar um chamado encerrado.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(descricao),
                "Descrição é obrigatória.");

            Descricao = descricao;
        }

        public void AlterarEquipamentoAfetado(string equipamentoAfetado)
        {
            DomainExceptionValidation.when(
                Status == StatusChamadoEnum.Fechado ||
                Status == StatusChamadoEnum.Cancelado,
                "Não é possível alterar um chamado encerrado.");

            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(equipamentoAfetado),
                "Equipamento afetado é obrigatório.");

            EquipamentoAfetado = equipamentoAfetado;
        }

        public void AlterarCategoria(
            int categoriaAtendimentoId,
            PrioridadeEnum prioridade)
        {
            DomainExceptionValidation.when(
                Status == StatusChamadoEnum.Fechado ||
                Status == StatusChamadoEnum.Cancelado,
                "Não é possível alterar um chamado encerrado.");

            DomainExceptionValidation.when(
                categoriaAtendimentoId <= 0,
                "Categoria inválida.");

            CategoriaAtendimentoId = categoriaAtendimentoId;
            Prioridade = prioridade;
        }

        // RELACIONAMENTOS E AUDITORIA

        public void AdicionarComentario(Comentario comentario)
        {
            DomainExceptionValidation.when(
                Status == StatusChamadoEnum.Fechado ||
                Status == StatusChamadoEnum.Cancelado,
                "Não é possível comentar em um chamado encerrado.");

            DomainExceptionValidation.when(
                comentario == null,
                "Comentário é obrigatório.");

            Comentarios.Add(comentario);
        }

        // Registra um evento de negócio para ser salvo na mesma operação
        // das demais alterações realizadas no chamado.
        public AuditoriaChamado RegistrarAuditoria(
            EventoAuditoriaChamadoEnum evento,
            string? usuarioId,
            string usuarioNome,
            string? perfilUsuario,
            StatusChamadoEnum? statusAnterior,
            StatusChamadoEnum? statusNovo,
            string descricao,
            string? detalhesAnteriores = null,
            string? detalhesNovos = null)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(Numero),
                "O chamado precisa possuir um número antes de registrar a auditoria.");

            var auditoria = new AuditoriaChamado(
                this,
                Numero!,
                evento,
                usuarioId,
                usuarioNome,
                perfilUsuario,
                statusAnterior,
                statusNovo,
                descricao,
                detalhesAnteriores,
                detalhesNovos);

            Auditorias.Add(auditoria);

            return auditoria;
        }

        // ENCERRAMENTO

        // Finaliza uma pausa aberta antes de encerrar definitivamente o chamado.
        public void Fechar(
            TipoAtendimentoEnum tipoAtendimento,
            TipoEncerramentoEnum tipoEncerramento,
            string? solucao)
        {
            DomainExceptionValidation.when(
                Status != StatusChamadoEnum.EmAtendimento &&
                Status != StatusChamadoEnum.AguardandoRetorno &&
                Status != StatusChamadoEnum.AguardandoEquipamento,
                "Somente chamados em atendimento, aguardando retorno ou aguardando equipamento podem ser encerrados.");

            if (tipoEncerramento == TipoEncerramentoEnum.Resolvido)
            {
                DomainExceptionValidation.when(
                    string.IsNullOrWhiteSpace(solucao),
                    "Solução é obrigatória para chamados resolvidos");
            }

            var agora = DateTime.Now;

            if (DataInicioPausaSla.HasValue)
            {
                var tempoPausa =
                    (agora - DataInicioPausaSla.Value).TotalHours;

                if (Status == StatusChamadoEnum.AguardandoRetorno)
                {
                    TempoPausadoHoras += tempoPausa;

                    TempoPausadoSlaRetornoHoras +=
                        CalcularHorasPausaSla(
                            DataInicioPausaSla.Value,
                            agora);
                }
                else if (Status == StatusChamadoEnum.AguardandoEquipamento)
                {
                    TempoPausadoEquipamentoHoras += tempoPausa;

                    TempoPausadoSlaEquipamentoHoras +=
                        CalcularHorasPausaSla(
                            DataInicioPausaSla.Value,
                            agora);
                }

                DataInicioPausaSla = null;
            }

            TipoAtendimento = tipoAtendimento;
            TipoEncerramento = tipoEncerramento;
            Solucao = solucao;

            Status = StatusChamadoEnum.Fechado;
            DataFechamento = agora;
        }

        public void Cancelar()
        {
            DomainExceptionValidation.when(
                Status == StatusChamadoEnum.Fechado,
                "Não é possivel cancelar um chamado fechado.");

            Status = StatusChamadoEnum.Cancelado;
        }

        // CONFIGURAÇÕES DO CHAMADO

        // Define o número gerado automaticamente pelo sistema.
        public void DefinirNumero(string numero)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(numero),
                "Número do chamado é obrigatório");

            Numero = numero;
        }

        public void DefinirResponsavel(string tecnicoId)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(tecnicoId),
                "Técnico responsável é obrigatório.");

            TecnicoResponsavelId = tecnicoId;
        }

        // Vincula uma fotografia versionada do período de trabalho ao chamado.
        public void DefinirPeriodoSla(PeriodoTrabalhoPerfil periodo)
        {
            DomainExceptionValidation.when(
                periodo == null,
                "Período de trabalho é obrigatório.");

            DomainExceptionValidation.when(
                PeriodoTrabalhoPerfilId.HasValue ||
                PeriodoTrabalhoPerfil != null,
                "O período de trabalho do chamado já foi definido.");

            DomainExceptionValidation.when(
                periodo!.Versao <= 0,
                "O período de trabalho precisa estar vinculado a um perfil.");

            DomainExceptionValidation.when(
                !periodo.EstaVigente(),
                "Somente o período de trabalho vigente pode ser definido no chamado.");

            PeriodoTrabalhoPerfil = periodo;
            PeriodoTrabalhoPerfilId =
                periodo.Id_PeriodoTrabalhoPerfil;
        }

        // CÁLCULOS DE TEMPO

        // Calcula o tempo total corrido desde a abertura do chamado.
        // Para chamados encerrados, utiliza a data de fechamento.
        // Para chamados ativos, utiliza a data atual recebida como parâmetro.
        // Esse tempo não desconta nenhuma pausa.
        public double CalcularTempoCorridoHoras(DateTime agora)
        {
            var dataReferencia = DataFechamento ?? agora;

            var tempoCorrido =
                (dataReferencia - DataAbertura).TotalHours;

            return Math.Max(0, tempoCorrido);
        }

        // Calcula quanto tempo passou entre a abertura do chamado
        // e o primeiro atendimento realizado pelo técnico.
        // Retorna null quando o chamado ainda não foi atendido.
        public double? CalcularTempoPrimeiroAtendimentoHoras()
        {
            if (!DataPrimeiroAtendimento.HasValue)
            {
                return null;
            }

            var tempoPrimeiroAtendimento =
                (DataPrimeiroAtendimento.Value -
                 DataAbertura).TotalHours;

            return Math.Max(0, tempoPrimeiroAtendimento);
        }

        // Calcula o tempo válido para o SLA de primeiro atendimento.
        // Enquanto o atendimento ainda não ocorreu, utiliza a data atual
        // para permitir o acompanhamento do prazo em tempo real.
        // Não desconta pausas, pois elas acontecem somente depois que o
        // chamado foi assumido. Retorna null quando não possui período.
        public double? CalcularTempoPrimeiroAtendimentoSlaHoras(
            DateTime agora)
        {
            var periodo = ObterPeriodoTrabalhoCarregado();

            if (periodo == null)
            {
                return null;
            }

            var dataReferencia =
                DataPrimeiroAtendimento ?? agora;

            return periodo.CalcularHorasNoPeriodo(
                DataAbertura,
                dataReferencia);
        }

        // Calcula o tempo total que o chamado ficou aguardando
        // uma resposta do solicitante.
        // Soma o tempo já acumulado com uma eventual pausa
        // que ainda esteja aberta em AguardandoRetorno.
        public double CalcularTempoAguardandoRetornoHoras(
            DateTime agora)
        {
            var tempoTotal = TempoPausadoHoras;

            if (Status == StatusChamadoEnum.AguardandoRetorno &&
                DataInicioPausaSla.HasValue)
            {
                var pausaAtual =
                    (agora -
                     DataInicioPausaSla.Value).TotalHours;

                tempoTotal += Math.Max(0, pausaAtual);
            }

            return Math.Max(0, tempoTotal);
        }

        // Calcula o tempo total que o chamado ficou aguardando
        // a chegada do equipamento na matriz.
        // Soma o tempo já acumulado com uma eventual pausa
        // que ainda esteja aberta em AguardandoEquipamento.
        public double CalcularTempoAguardandoEquipamentoHoras(
            DateTime agora)
        {
            var tempoTotal =
                TempoPausadoEquipamentoHoras;

            if (Status ==
                    StatusChamadoEnum.AguardandoEquipamento &&
                DataInicioPausaSla.HasValue)
            {
                var pausaAtual =
                    (agora -
                     DataInicioPausaSla.Value).TotalHours;

                tempoTotal += Math.Max(0, pausaAtual);
            }

            return Math.Max(0, tempoTotal);
        }

        // Calcula o tempo total de pausa do SLA.
        // Soma o tempo aguardando o solicitante com o tempo
        // aguardando a chegada do equipamento.
        public double CalcularTempoTotalPausadoHoras(
            DateTime agora)
        {
            var tempoAguardandoRetorno =
                CalcularTempoAguardandoRetornoHoras(agora);

            var tempoAguardandoEquipamento =
                CalcularTempoAguardandoEquipamentoHoras(agora);

            return tempoAguardandoRetorno +
                   tempoAguardandoEquipamento;
        }

        // Calcula o tempo válido para o SLA de resolução.
        // Soma somente as horas dentro do período de trabalho e
        // desconta as pausas de retorno e equipamento nesse período.
        // Retorna null enquanto o chamado não possuir um período.
        public double? CalcularTempoResolucaoSlaHoras(
            DateTime agora)
        {
            var periodo = ObterPeriodoTrabalhoCarregado();

            if (periodo == null)
            {
                return null;
            }

            var dataReferencia = DataFechamento ?? agora;

            var tempoDentroDoPeriodo =
                periodo.CalcularHorasNoPeriodo(
                    DataAbertura,
                    dataReferencia);

            var tempoPausado =
                CalcularTempoTotalPausadoSlaHoras(agora);

            return Math.Max(
                0,
                tempoDentroDoPeriodo - tempoPausado);
        }

        // MÉTODOS INTERNOS DE SLA

        // Soma as pausas já concluídas com uma eventual pausa ainda aberta.
        private double CalcularTempoTotalPausadoSlaHoras(
            DateTime agora)
        {
            var tempoTotal =
                TempoPausadoSlaRetornoHoras +
                TempoPausadoSlaEquipamentoHoras;

            if (DataInicioPausaSla.HasValue &&
                (Status == StatusChamadoEnum.AguardandoRetorno ||
                 Status == StatusChamadoEnum.AguardandoEquipamento))
            {
                tempoTotal += CalcularHorasPausaSla(
                    DataInicioPausaSla.Value,
                    agora);
            }

            return Math.Max(0, tempoTotal);
        }

        // Calcula a parcela da pausa que ocorreu dentro do período de trabalho.
        private double CalcularHorasPausaSla(
            DateTime inicio,
            DateTime fim)
        {
            var periodo = ObterPeriodoTrabalhoCarregado();

            return periodo?.CalcularHorasNoPeriodo(
                       inicio,
                       fim) ?? 0;
        }

        // Garante que o período versionado tenha sido carregado quando existe.
        private PeriodoTrabalhoPerfil? ObterPeriodoTrabalhoCarregado()
        {
            DomainExceptionValidation.when(
                PeriodoTrabalhoPerfilId.HasValue &&
                PeriodoTrabalhoPerfil == null,
                "O período de trabalho do chamado não foi carregado.");

            return PeriodoTrabalhoPerfil;
        }

        // Calcula o tempo efetivo de atendimento do técnico.
        // A contagem começa no primeiro atendimento e desconta
        // as pausas de retorno e de equipamento.
        // Retorna null quando o chamado ainda não foi atendido.
        public double? CalcularTempoEfetivoAtendimentoHoras(
            DateTime agora)
        {
            if (!DataPrimeiroAtendimento.HasValue)
            {
                return null;
            }

            var dataReferencia = DataFechamento ?? agora;

            var tempoDesdePrimeiroAtendimento =
                (dataReferencia -
                 DataPrimeiroAtendimento.Value).TotalHours;

            var tempoPausado =
                CalcularTempoTotalPausadoHoras(agora);

            return Math.Max(
                0,
                tempoDesdePrimeiroAtendimento -
                tempoPausado);
        }

    }
}
