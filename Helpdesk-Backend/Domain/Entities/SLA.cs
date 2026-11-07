using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Define os limites de primeiro atendimento e resolução para uma
    // prioridade de chamado.
    public class SLA
    {
        public int Id_SLA { get; private set; }
        public PrioridadeEnum Prioridade { get; private set; }
        public int TempoAtendimentoHoras { get; private set; }
        public int TempoResolucaoHoras { get; private set; }
        public bool Ativo { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected SLA()
        {
        }

        public SLA(
            PrioridadeEnum prioridade,
            int tempoAtendimentoHoras,
            int tempoResolucaoHoras)
        {
            Validate(
                prioridade,
                tempoAtendimentoHoras,
                tempoResolucaoHoras);

            Prioridade = prioridade;
            TempoAtendimentoHoras = tempoAtendimentoHoras;
            TempoResolucaoHoras = tempoResolucaoHoras;
            Ativo = true;
        }

        // CONFIGURAÇÃO E ESTADO

        public void Atualizar(
            int tempoAtendimentoHoras,
            int tempoResolucaoHoras)
        {
            Validate(
                Prioridade,
                tempoAtendimentoHoras,
                tempoResolucaoHoras);

            TempoAtendimentoHoras = tempoAtendimentoHoras;
            TempoResolucaoHoras = tempoResolucaoHoras;
        }

        public void Ativar()
        {
            Ativo = true;
        }

        public void Desativar()
        {
            Ativo = false;
        }

        // CÁLCULOS

        // Calcula o intervalo entre a abertura e o primeiro atendimento.
        public TimeSpan TempoAtendidmento(
            DateTime dataAbertura,
            DateTime dataAtendimento)
        {
            DomainExceptionValidation.when(
                dataAtendimento < dataAbertura,
                "A data de atendimento não pode ser menor que a data de abertura.");

            return dataAtendimento - dataAbertura;
        }

        // Calcula o intervalo de resolução descontando as pausas recebidas.
        public TimeSpan TempoResolucao(
            DateTime dataAbertura,
            DateTime dataEncerramento,
            double tempoPausadoHoras)
        {
            DomainExceptionValidation.when(
                dataEncerramento < dataAbertura,
                "A data de encerramento não pode ser menor que a data de abertura.");

            return (dataEncerramento - dataAbertura) -
                   TimeSpan.FromHours(tempoPausadoHoras);
        }

        public bool AtendimentoDentroPrazo(TimeSpan tempoAtendimento)
        {
            return tempoAtendimento.TotalHours <=
                   TempoAtendimentoHoras;
        }

        public bool ResolucaoDentroPrazo(TimeSpan tempoResolucao)
        {
            return tempoResolucao.TotalHours <=
                   TempoResolucaoHoras;
        }

        // VALIDAÇÃO

        // Garante limites positivos e impede que a resolução seja menor que
        // o prazo de primeiro atendimento.
        private void Validate(
            PrioridadeEnum prioridade,
            int tempoAtendimentoHoras,
            int tempoResolucaoHoras)
        {
            DomainExceptionValidation.when(
                tempoAtendimentoHoras <= 0,
                "Tempo de atendimento deve ser maior que zero.");

            DomainExceptionValidation.when(
                tempoResolucaoHoras <= 0,
                "Tempo de resolução deve ser maior que zero.");

            DomainExceptionValidation.when(
                tempoResolucaoHoras < tempoAtendimentoHoras,
                "O tempo de resolução não pode ser menor que o tempo de atendimento.");
        }
    }
}
