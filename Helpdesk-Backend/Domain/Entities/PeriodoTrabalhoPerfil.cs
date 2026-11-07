using Domain.Validation;

namespace Domain.Entities
{
    public class PeriodoTrabalhoPerfil
    {
        public int Id_PeriodoTrabalhoPerfil { get; private set; }
        public int PerfilId { get; private set; }
        public int Versao { get; private set; }
        public DateTime DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }
        public bool SegundaAtivo { get; private set; }
        public bool TercaAtivo { get; private set; }
        public bool QuartaAtivo { get; private set; }
        public bool QuintaAtivo { get; private set; }
        public bool SextaAtivo { get; private set; }
        public TimeSpan? HoraInicioSemana { get; private set; }
        public TimeSpan? HoraFimSemana { get; private set; }
        public bool SabadoAtivo { get; private set; }
        public TimeSpan? HoraInicioSabado { get; private set; }
        public TimeSpan? HoraFimSabado { get; private set; }
        public bool DomingoAtivo { get; private set; }
        public TimeSpan? HoraInicioDomingo { get; private set; }
        public TimeSpan? HoraFimDomingo { get; private set; }
        public Perfil? Perfil { get; private set; }
        public ICollection<Chamado> Chamados { get; private set; }
            = new List<Chamado>();

        protected PeriodoTrabalhoPerfil() { }

        public PeriodoTrabalhoPerfil(
            bool segundaAtivo,
            bool tercaAtivo,
            bool quartaAtivo,
            bool quintaAtivo,
            bool sextaAtivo,
            TimeSpan? horaInicioSemana,
            TimeSpan? horaFimSemana,
            bool sabadoAtivo,
            TimeSpan? horaInicioSabado,
            TimeSpan? horaFimSabado,
            bool domingoAtivo,
            TimeSpan? horaInicioDomingo,
            TimeSpan? horaFimDomingo)
        {
            ValidarConfiguracao(
                segundaAtivo,
                tercaAtivo,
                quartaAtivo,
                quintaAtivo,
                sextaAtivo,
                horaInicioSemana,
                horaFimSemana,
                sabadoAtivo,
                horaInicioSabado,
                horaFimSabado,
                domingoAtivo,
                horaInicioDomingo,
                horaFimDomingo);

            SegundaAtivo = segundaAtivo;
            TercaAtivo = tercaAtivo;
            QuartaAtivo = quartaAtivo;
            QuintaAtivo = quintaAtivo;
            SextaAtivo = sextaAtivo;

            var possuiDiaSemanaAtivo = PossuiDiaSemanaAtivo();

            HoraInicioSemana = possuiDiaSemanaAtivo
                ? horaInicioSemana
                : null;
            HoraFimSemana = possuiDiaSemanaAtivo
                ? horaFimSemana
                : null;

            SabadoAtivo = sabadoAtivo;
            HoraInicioSabado = sabadoAtivo
                ? horaInicioSabado
                : null;
            HoraFimSabado = sabadoAtivo
                ? horaFimSabado
                : null;

            DomingoAtivo = domingoAtivo;
            HoraInicioDomingo = domingoAtivo
                ? horaInicioDomingo
                : null;
            HoraFimDomingo = domingoAtivo
                ? horaFimDomingo
                : null;
        }

        public bool EstaVigente()
        {
            return !DataFimVigencia.HasValue;
        }

        public void EncerrarVigencia(DateTime agora)
        {
            DomainExceptionValidation.when(
                !EstaVigente(),
                "O período de trabalho já foi encerrado.");

            DomainExceptionValidation.when(
                agora < DataInicioVigencia,
                "O fim da vigência não pode ser anterior ao início.");

            DataFimVigencia = agora;
        }

        public bool PossuiMesmaConfiguracao(
            PeriodoTrabalhoPerfil outro)
        {
            if (outro == null)
            {
                return false;
            }

            return SegundaAtivo == outro.SegundaAtivo &&
                   TercaAtivo == outro.TercaAtivo &&
                   QuartaAtivo == outro.QuartaAtivo &&
                   QuintaAtivo == outro.QuintaAtivo &&
                   SextaAtivo == outro.SextaAtivo &&
                   HoraInicioSemana == outro.HoraInicioSemana &&
                   HoraFimSemana == outro.HoraFimSemana &&
                   SabadoAtivo == outro.SabadoAtivo &&
                   HoraInicioSabado == outro.HoraInicioSabado &&
                   HoraFimSabado == outro.HoraFimSabado &&
                   DomingoAtivo == outro.DomingoAtivo &&
                   HoraInicioDomingo == outro.HoraInicioDomingo &&
                   HoraFimDomingo == outro.HoraFimDomingo;
        }

        // Calcula somente as horas do intervalo que coincidem com
        // os turnos configurados, incluindo turnos que terminam
        // no dia seguinte e evitando duplicidade em sobreposições.
        public double CalcularHorasNoPeriodo(
            DateTime inicio,
            DateTime fim)
        {
            if (fim <= inicio)
            {
                return 0;
            }

            var janelas = new List<(DateTime Inicio, DateTime Fim)>();
            var primeiroDia = inicio.Date.AddDays(-1);
            var ultimoDia = fim.Date;

            for (var dia = primeiroDia;
                 dia <= ultimoDia;
                 dia = dia.AddDays(1))
            {
                var horario = ObterHorario(dia.DayOfWeek);

                if (!horario.Ativo ||
                    !horario.Inicio.HasValue ||
                    !horario.Fim.HasValue)
                {
                    continue;
                }

                var inicioTurno =
                    dia.Add(horario.Inicio.Value);
                var fimTurno =
                    dia.Add(horario.Fim.Value);

                if (horario.Fim.Value < horario.Inicio.Value)
                {
                    fimTurno = fimTurno.AddDays(1);
                }

                if (fimTurno <= inicio || inicioTurno >= fim)
                {
                    continue;
                }

                janelas.Add((inicioTurno, fimTurno));
            }

            if (janelas.Count == 0)
            {
                return 0;
            }

            var janelasOrdenadas = janelas
                .OrderBy(janela => janela.Inicio)
                .ToList();

            var inicioAtual = janelasOrdenadas[0].Inicio;
            var fimAtual = janelasOrdenadas[0].Fim;
            var totalHoras = 0d;

            foreach (var janela in janelasOrdenadas.Skip(1))
            {
                if (janela.Inicio <= fimAtual)
                {
                    if (janela.Fim > fimAtual)
                    {
                        fimAtual = janela.Fim;
                    }

                    continue;
                }

                totalHoras += CalcularIntersecaoHoras(
                    inicioAtual,
                    fimAtual,
                    inicio,
                    fim);

                inicioAtual = janela.Inicio;
                fimAtual = janela.Fim;
            }

            totalHoras += CalcularIntersecaoHoras(
                inicioAtual,
                fimAtual,
                inicio,
                fim);

            return Math.Max(0, totalHoras);
        }

        internal void VincularAoPerfil(
            Perfil perfil,
            int versao,
            DateTime dataInicioVigencia)
        {
            DomainExceptionValidation.when(
                perfil == null,
                "Perfil é obrigatório.");

            DomainExceptionValidation.when(
                Versao > 0 || Perfil != null,
                "O período de trabalho já pertence a um perfil.");

            DomainExceptionValidation.when(
                versao <= 0,
                "A versão do período deve ser maior que zero.");

            Perfil = perfil!;
            PerfilId = perfil!.Id_Perfil;
            Versao = versao;
            DataInicioVigencia = dataInicioVigencia;
        }

        private bool PossuiDiaSemanaAtivo()
        {
            return SegundaAtivo ||
                   TercaAtivo ||
                   QuartaAtivo ||
                   QuintaAtivo ||
                   SextaAtivo;
        }

        private (bool Ativo, TimeSpan? Inicio, TimeSpan? Fim)
            ObterHorario(DayOfWeek diaSemana)
        {
            return diaSemana switch
            {
                DayOfWeek.Monday =>
                    (SegundaAtivo, HoraInicioSemana, HoraFimSemana),
                DayOfWeek.Tuesday =>
                    (TercaAtivo, HoraInicioSemana, HoraFimSemana),
                DayOfWeek.Wednesday =>
                    (QuartaAtivo, HoraInicioSemana, HoraFimSemana),
                DayOfWeek.Thursday =>
                    (QuintaAtivo, HoraInicioSemana, HoraFimSemana),
                DayOfWeek.Friday =>
                    (SextaAtivo, HoraInicioSemana, HoraFimSemana),
                DayOfWeek.Saturday =>
                    (SabadoAtivo, HoraInicioSabado, HoraFimSabado),
                DayOfWeek.Sunday =>
                    (DomingoAtivo, HoraInicioDomingo, HoraFimDomingo),
                _ => (false, null, null)
            };
        }

        private static double CalcularIntersecaoHoras(
            DateTime inicioJanela,
            DateTime fimJanela,
            DateTime inicioIntervalo,
            DateTime fimIntervalo)
        {
            var inicioValido = inicioJanela > inicioIntervalo
                ? inicioJanela
                : inicioIntervalo;
            var fimValido = fimJanela < fimIntervalo
                ? fimJanela
                : fimIntervalo;

            return fimValido <= inicioValido
                ? 0
                : (fimValido - inicioValido).TotalHours;
        }

        private static void ValidarConfiguracao(
            bool segundaAtivo,
            bool tercaAtivo,
            bool quartaAtivo,
            bool quintaAtivo,
            bool sextaAtivo,
            TimeSpan? horaInicioSemana,
            TimeSpan? horaFimSemana,
            bool sabadoAtivo,
            TimeSpan? horaInicioSabado,
            TimeSpan? horaFimSabado,
            bool domingoAtivo,
            TimeSpan? horaInicioDomingo,
            TimeSpan? horaFimDomingo)
        {
            var possuiDiaSemanaAtivo =
                segundaAtivo ||
                tercaAtivo ||
                quartaAtivo ||
                quintaAtivo ||
                sextaAtivo;

            DomainExceptionValidation.when(
                !possuiDiaSemanaAtivo &&
                !sabadoAtivo &&
                !domingoAtivo,
                "Pelo menos um dia de trabalho deve estar ativo.");

            if (possuiDiaSemanaAtivo)
            {
                ValidarHorario(
                    horaInicioSemana,
                    horaFimSemana,
                    "dias de segunda a sexta");
            }

            if (sabadoAtivo)
            {
                ValidarHorario(
                    horaInicioSabado,
                    horaFimSabado,
                    "sábado");
            }

            if (domingoAtivo)
            {
                ValidarHorario(
                    horaInicioDomingo,
                    horaFimDomingo,
                    "domingo");
            }
        }

        private static void ValidarHorario(
            TimeSpan? inicio,
            TimeSpan? fim,
            string periodo)
        {
            DomainExceptionValidation.when(
                !inicio.HasValue || !fim.HasValue,
                $"Os horários de {periodo} são obrigatórios.");

            DomainExceptionValidation.when(
                inicio < TimeSpan.Zero ||
                inicio >= TimeSpan.FromDays(1) ||
                fim < TimeSpan.Zero ||
                fim >= TimeSpan.FromDays(1),
                $"Os horários de {periodo} devem estar entre 00:00 e 23:59.");

            DomainExceptionValidation.when(
                inicio == fim,
                $"O horário inicial e final de {periodo} não podem ser iguais.");
        }
    }
}
