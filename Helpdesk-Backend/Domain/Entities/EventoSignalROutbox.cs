using Domain.Enum;
using Domain.Validation;

namespace Domain.Entities
{
    // Representa um evento SignalR persistido no banco para publicação
    // somente depois que a operação de negócio for confirmada.
    public class EventoSignalROutbox
    {
        private const int TamanhoMaximoNomeEvento = 100;
        private const int TamanhoMaximoGrupoDestino = 100;
        private const int TamanhoMaximoErro = 4000;
        private const int MaximoTentativas = 10;
        private const int AtrasoInicialSegundos = 5;
        private const int AtrasoMaximoSegundos = 300;
        public int Id_EventoSignalROutbox { get; private set; }
        public string NomeEvento { get; private set; }
        public TipoDestinoEventoSignalREnum TipoDestino { get; private set; }
        public string? DestinatariosJson { get; private set; }
        public string? GrupoDestino { get; private set; }
        public string? PayloadJson { get; private set; }
        public StatusEventoSignalROutboxEnum Status { get; private set; }
        public int QuantidadeTentativas { get; private set; }
        public DateTime DataCriacaoUtc { get; private set; }
        public DateTime ProximaTentativaUtc { get; private set; }
        public DateTime? ProcessamentoIniciadoUtc { get; private set; }
        public DateTime? DataEnvioUtc { get; private set; }
        public string? UltimoErro { get; private set; }

        // Construtor reservado para materialização pelo Entity Framework.
        protected EventoSignalROutbox()
        {
        }

        public EventoSignalROutbox(
            string nomeEvento,
            TipoDestinoEventoSignalREnum tipoDestino,
            string? destinatariosJson,
            string? grupoDestino,
            string? payloadJson)
        {
            ValidarDominio(
                nomeEvento,
                tipoDestino,
                destinatariosJson,
                grupoDestino);

            var agoraUtc = DateTime.UtcNow;

            NomeEvento = nomeEvento.Trim();
            TipoDestino = tipoDestino;

            DestinatariosJson =
                string.IsNullOrWhiteSpace(destinatariosJson)
                    ? null
                    : destinatariosJson;

            GrupoDestino =
                string.IsNullOrWhiteSpace(grupoDestino)
                    ? null
                    : grupoDestino.Trim();

            PayloadJson =
                string.IsNullOrWhiteSpace(payloadJson)
                    ? null
                    : payloadJson;

            Status = StatusEventoSignalROutboxEnum.Pendente;
            QuantidadeTentativas = 0;
            DataCriacaoUtc = agoraUtc;
            ProximaTentativaUtc = agoraUtc;
        }

        // Valida os dados obrigatórios e garante que o evento possua
        // usuários ou grupo como destino, nunca os dois simultaneamente.
        private static void ValidarDominio(
            string nomeEvento,
            TipoDestinoEventoSignalREnum tipoDestino,
            string? destinatariosJson,
            string? grupoDestino)
        {
            DomainExceptionValidation.when(
                string.IsNullOrWhiteSpace(nomeEvento),
                "O nome do evento SignalR é obrigatório.");

            DomainExceptionValidation.when(
                nomeEvento?.Trim().Length > TamanhoMaximoNomeEvento,
                $"O nome do evento SignalR deve possuir no máximo {TamanhoMaximoNomeEvento} caracteres.");

            DomainExceptionValidation.when(
                tipoDestino != TipoDestinoEventoSignalREnum.Usuarios &&
                tipoDestino != TipoDestinoEventoSignalREnum.Grupo,
                "O tipo de destino do evento SignalR é inválido.");

            if (tipoDestino == TipoDestinoEventoSignalREnum.Usuarios)
            {
                DomainExceptionValidation.when(
                    string.IsNullOrWhiteSpace(destinatariosJson),
                    "O evento destinado a usuários deve possuir destinatários.");

                DomainExceptionValidation.when(
                    !string.IsNullOrWhiteSpace(grupoDestino),
                    "O evento destinado a usuários não pode possuir grupo.");
            }

            if (tipoDestino == TipoDestinoEventoSignalREnum.Grupo)
            {
                DomainExceptionValidation.when(
                    string.IsNullOrWhiteSpace(grupoDestino),
                    "O evento destinado a grupo deve possuir um grupo.");

                DomainExceptionValidation.when(
                    grupoDestino?.Trim().Length > TamanhoMaximoGrupoDestino,
                    $"O grupo do evento SignalR deve possuir no máximo {TamanhoMaximoGrupoDestino} caracteres.");

                DomainExceptionValidation.when(
                    !string.IsNullOrWhiteSpace(destinatariosJson),
                    "O evento destinado a grupo não pode possuir usuários.");
            }
        }

        // Marca o evento como reservado pelo processador para evitar
        // que ele seja publicado simultaneamente por outra instância.
        public void MarcarProcessando()
        {
            DomainExceptionValidation.when(
                Status != StatusEventoSignalROutboxEnum.Pendente,
                "Somente eventos pendentes podem iniciar o processamento.");

            Status = StatusEventoSignalROutboxEnum.Processando;
            ProcessamentoIniciadoUtc = DateTime.UtcNow;
        }

        // Confirma que o evento foi publicado no SignalR com sucesso.
        public void MarcarEnviado()
        {
            DomainExceptionValidation.when(
                Status != StatusEventoSignalROutboxEnum.Processando,
                "Somente eventos em processamento podem ser marcados como enviados.");

            Status = StatusEventoSignalROutboxEnum.Enviado;
            DataEnvioUtc = DateTime.UtcNow;
            ProcessamentoIniciadoUtc = null;
            UltimoErro = null;
        }

        // Registra uma falha de publicação, calcula a próxima tentativa
        // e interrompe novas tentativas automáticas após o limite definido.
        public void RegistrarFalha(string? erro)
        {
            DomainExceptionValidation.when(
                Status != StatusEventoSignalROutboxEnum.Processando,
                "Somente eventos em processamento podem registrar falha.");

            QuantidadeTentativas++;
            ProcessamentoIniciadoUtc = null;
            UltimoErro = LimitarMensagemErro(erro);

            if (QuantidadeTentativas >= MaximoTentativas)
            {
                Status = StatusEventoSignalROutboxEnum.Falhou;
                return;
            }

            Status = StatusEventoSignalROutboxEnum.Pendente;

            var atrasoSegundos =
                CalcularAtrasoProximaTentativa(QuantidadeTentativas);

            ProximaTentativaUtc =
                DateTime.UtcNow.AddSeconds(atrasoSegundos);
        }

        // Libera manualmente um evento que atingiu o limite de tentativas,
        // permitindo que ele entre novamente no processamento automático.
        public void Reagendar()
        {
            DomainExceptionValidation.when(
                Status != StatusEventoSignalROutboxEnum.Falhou,
                "Somente eventos com falha podem ser reagendados.");

            Status = StatusEventoSignalROutboxEnum.Pendente;
            QuantidadeTentativas = 0;
            ProximaTentativaUtc = DateTime.UtcNow;
            ProcessamentoIniciadoUtc = null;
            DataEnvioUtc = null;
            UltimoErro = null;
        }

        // Recupera um evento que ficou travado como Processando após uma
        // interrupção ou encerramento inesperado da aplicação.
        public void RecuperarProcessamento()
        {
            DomainExceptionValidation.when(
                Status != StatusEventoSignalROutboxEnum.Processando,
                "Somente eventos em processamento podem ser recuperados.");

            Status = StatusEventoSignalROutboxEnum.Pendente;
            ProcessamentoIniciadoUtc = null;
            ProximaTentativaUtc = DateTime.UtcNow;
        }

        // Calcula a espera progressiva de 5, 10, 20, 40, 80 e 160 segundos,
        // respeitando o limite máximo de 300 segundos.
        private static int CalcularAtrasoProximaTentativa(
            int quantidadeTentativas)
        {
            var atrasoCalculado =
                AtrasoInicialSegundos *
                Math.Pow(
                    2,
                    Math.Max(
                        0,
                        quantidadeTentativas - 1));

            return (int)Math.Min(
                atrasoCalculado,
                AtrasoMaximoSegundos);
        }

        // Mantém a mensagem de erro dentro do limite suportado
        // pela coluna do banco de dados.
        private static string LimitarMensagemErro(
            string? erro)
        {
            var mensagem =
                string.IsNullOrWhiteSpace(erro)
                    ? "Falha não informada durante a publicação do evento SignalR."
                    : erro.Trim();

            return mensagem.Length <= TamanhoMaximoErro
                ? mensagem
                : mensagem[..TamanhoMaximoErro];
        }
    }
}