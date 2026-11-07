using Application.Interfaces;
using Application.ViewModel;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Application.Services
{
    // Mantém em memória os dados operacionais utilizados pelo painel de
    // monitoramento: requisições, erros, incidentes e endpoints lentos.
    public class MonitoramentoService : IMonitoramentoService
    {
        // LIMITES E PERÍODOS DO MONITORAMENTO

        private const int HorasRetencao = 24;
        private const int MinutosIncidenteAtivo = 5;
        private const double TempoEndpointLentoMs = 2000;
        private const int QuantidadeMaximaErros = 10000;
        private const int LimiteIncidentesPainel = 20;
        private const int LimiteErrosPainel = 100;
        private const int LimiteEndpointsPainel = 20;
        private const int MinutosResetContadorRequisicoes = 60;

        private DateTime _inicioContagemRequisicoes = DateTime.UtcNow;
        private readonly object _bloqueioContadorRequisicoes = new();

        private readonly ConcurrentQueue<ErroMonitoramentoViewModel>
            _erros = new();

        private readonly ConcurrentDictionary<string, IncidenteErroViewModel>
            _incidentes = new();

        private readonly ConcurrentDictionary<string, EndpointLentoViewModel>
            _endpointsLentos = new();

        private readonly ConcurrentDictionary<string, DateTime>
            _traceIdsComErroTecnico = new();

        private long _totalRequisicoes;

        // REGISTROS

        // Contabiliza a requisição, identifica endpoints lentos e transforma
        // respostas HTTP com erro em registros do monitoramento.
        public Task RegistrarRequisicao(
            string metodoHttp,
            string endpoint,
            int statusCode,
            double tempoRespostaMs,
            string? traceId)
        {
            ResetarContadorRequisicoesSeNecessario();

            metodoHttp = NormalizarValor(
                metodoHttp,
                "DESCONHECIDO");

            endpoint = NormalizarValor(
                endpoint,
                "Endpoint não identificado");

            if (!string.Equals(
                    endpoint.TrimStart('/'),
                    "api/Monitoramento/painel",
                    StringComparison.OrdinalIgnoreCase))
            {
                Interlocked.Increment(ref _totalRequisicoes);
            }

            if (tempoRespostaMs >= TempoEndpointLentoMs)
            {
                RegistrarEndpointLento(
                    metodoHttp,
                    endpoint,
                    tempoRespostaMs);
            }

            if (statusCode < 400)
            {
                return Task.CompletedTask;
            }

            // Se o ILogger já registrou a exceção técnica,
            // evita registrar outro erro genérico para o mesmo TraceId.
            if (!string.IsNullOrWhiteSpace(traceId) &&
                _traceIdsComErroTecnico.ContainsKey(traceId))
            {
                return Task.CompletedTask;
            }

            return RegistrarErro(
                "Requisição HTTP",
                $"A requisição retornou o status HTTP {statusCode}.",
                null,
                metodoHttp,
                endpoint,
                statusCode,
                traceId);
        }

        // Registra os detalhes técnicos de um erro, atualiza seu incidente e
        // limita o histórico em memória para evitar crescimento indefinido.
        public Task RegistrarErro(
            string origem,
            string mensagem,
            Exception? exception,
            string? metodoHttp,
            string? endpoint,
            int? statusCode,
            string? traceId)
        {
            var agora = DateTime.UtcNow;

            origem = NormalizarValor(
                origem,
                "Aplicação");

            metodoHttp = NormalizarValorOuNulo(metodoHttp);
            endpoint = NormalizarValorOuNulo(endpoint);

            var tipoErro = exception?.GetType().Name
                ?? "ErroHttp";

            var codigoErro = ObterCodigoErro(exception);

            var mensagemCompleta = exception is null
                ? mensagem
                : $"{mensagem} {exception.Message}";

            mensagemCompleta = SanitizarMensagem(
                mensagemCompleta);

            var excecaoInterna = ObterExcecaoInterna(
                exception);

            var erro = new ErroMonitoramentoViewModel
            {
                Origem = origem,
                MetodoHttp = metodoHttp,
                Endpoint = endpoint,
                StatusCode = statusCode,
                TipoErro = tipoErro,
                CodigoErro = codigoErro,
                Mensagem = mensagemCompleta,
                ExcecaoInterna = excecaoInterna,
                TraceId = traceId,
                DataOcorrencia = agora
            };

            _erros.Enqueue(erro);

            if (exception is not null &&
                !string.IsNullOrWhiteSpace(traceId))
            {
                _traceIdsComErroTecnico[traceId] = agora;
            }

            RegistrarIncidente(erro);

            while (_erros.Count > QuantidadeMaximaErros)
            {
                _erros.TryDequeue(out _);
            }

            return Task.CompletedTask;
        }

        // CONSULTA DO PAINEL

        // Monta uma fotografia segura dos indicadores atuais sem expor os
        // objetos que continuam sendo atualizados pelas demais requisições.
        public Task<PainelErrosViewModel> ObterPainel()
        {
            ResetarContadorRequisicoesSeNecessario();
            RemoverExpiradosInterno();

            var agora = DateTime.UtcNow;
            var limiteAtivo = agora.AddMinutes(
                -MinutosIncidenteAtivo);

            var incidentesAtivos = _incidentes.Values
                .Where(x => x.UltimaOcorrencia >= limiteAtivo)
                .OrderByDescending(x => x.UltimaOcorrencia)
                .Take(LimiteIncidentesPainel)
                .Select(x => CopiarIncidente(x, agora))
                .ToList();

            var errosRecentes = _erros
                .OrderByDescending(x => x.DataOcorrencia)
                .Take(LimiteErrosPainel)
                .ToList();

            var endpointsLentos = _endpointsLentos.Values
                .OrderByDescending(x => x.TempoMedioMs)
                .ThenByDescending(x => x.MaiorTempoMs)
                .Take(LimiteEndpointsPainel)
                .Select(CopiarEndpoint)
                .ToList();

            var umaHoraAtras = agora.AddHours(-1);

            var painel = new PainelErrosViewModel
            {
                DataAtualizacao = agora,

                Resumo = new ResumoErrosViewModel
                {
                    IncidentesAtivos =
                        incidentesAtivos.Count,

                    ErrosUltimaHora = _erros.Count(
                        x => x.DataOcorrencia >= umaHoraAtras),

                    EndpointsLentos =
                        _endpointsLentos.Count,

                    TotalRequisicoesMonitoradas =
                        Convert.ToInt32(
                            Interlocked.Read(
                                ref _totalRequisicoes))
                },

                IncidentesAtivos = incidentesAtivos,
                ErrosRecentes = errosRecentes,
                EndpointsLentos = endpointsLentos
            };

            return Task.FromResult(painel);
        }

        // LIMPEZA

        // Remove manualmente os dados que ultrapassaram o período de retenção.
        public Task<int> RemoverExpirados()
        {
            var removidos = RemoverExpiradosInterno();

            return Task.FromResult(removidos);
        }

        // MÉTODOS INTERNOS

        // Agrupa ocorrências equivalentes em um único incidente.
        private void RegistrarIncidente(
            ErroMonitoramentoViewModel erro)
        {
            var identificador = CriarIdentificador(
                erro);

            _incidentes.AddOrUpdate(
                identificador,

                _ => new IncidenteErroViewModel
                {
                    Identificador = identificador,
                    Origem = erro.Origem,
                    MetodoHttp = erro.MetodoHttp,
                    Endpoint = erro.Endpoint,
                    StatusCode = erro.StatusCode,
                    TipoErro = erro.TipoErro,
                    CodigoErro = erro.CodigoErro,
                    Mensagem = erro.Mensagem,
                    PrimeiraOcorrencia =
                        erro.DataOcorrencia,
                    UltimaOcorrencia =
                        erro.DataOcorrencia,
                    QuantidadeOcorrencias = 1,
                    DuracaoAtivaSegundos = 0,
                    EstaAtivo = true
                },

                (_, incidente) =>
                {
                    lock (incidente)
                    {
                        incidente.UltimaOcorrencia =
                            erro.DataOcorrencia;

                        incidente.QuantidadeOcorrencias++;

                        incidente.DuracaoAtivaSegundos =
                            (incidente.UltimaOcorrencia -
                             incidente.PrimeiraOcorrencia)
                            .TotalSeconds;

                        incidente.Mensagem = erro.Mensagem;
                        incidente.EstaAtivo = true;
                    }

                    return incidente;
                });
        }

        // Consolida as medições de lentidão por método e endpoint.
        private void RegistrarEndpointLento(
            string metodoHttp,
            string endpoint,
            double tempoRespostaMs)
        {
            var agora = DateTime.UtcNow;
            var chave = $"{metodoHttp}:{endpoint}";

            _endpointsLentos.AddOrUpdate(
                chave,

                _ => new EndpointLentoViewModel
                {
                    MetodoHttp = metodoHttp,
                    Endpoint = endpoint,
                    QuantidadeOcorrencias = 1,
                    TempoMedioMs = tempoRespostaMs,
                    MaiorTempoMs = tempoRespostaMs,
                    UltimoTempoMs = tempoRespostaMs,
                    UltimaOcorrencia = agora
                },

                (_, endpointAtual) =>
                {
                    lock (endpointAtual)
                    {
                        var quantidadeAnterior =
                            endpointAtual.QuantidadeOcorrencias;

                        endpointAtual.QuantidadeOcorrencias++;

                        endpointAtual.TempoMedioMs =
                            ((endpointAtual.TempoMedioMs *
                              quantidadeAnterior) +
                             tempoRespostaMs) /
                            endpointAtual.QuantidadeOcorrencias;

                        endpointAtual.MaiorTempoMs =
                            Math.Max(
                                endpointAtual.MaiorTempoMs,
                                tempoRespostaMs);

                        endpointAtual.UltimoTempoMs =
                            tempoRespostaMs;

                        endpointAtual.UltimaOcorrencia =
                            agora;
                    }

                    return endpointAtual;
                });
        }

        // Elimina erros, incidentes, endpoints e TraceIds já expirados.
        private int RemoverExpiradosInterno()
        {
            var removidos = 0;

            var dataLimite = DateTime.UtcNow.AddHours(
                -HorasRetencao);

            while (_erros.TryPeek(out var erro) &&
                   erro.DataOcorrencia < dataLimite)
            {
                if (_erros.TryDequeue(out _))
                {
                    removidos++;
                }
            }

            foreach (var incidente in _incidentes)
            {
                if (incidente.Value.UltimaOcorrencia <
                    dataLimite &&
                    _incidentes.TryRemove(
                        incidente.Key,
                        out _))
                {
                    removidos++;
                }
            }

            foreach (var endpoint in _endpointsLentos)
            {
                if (endpoint.Value.UltimaOcorrencia <
                    dataLimite &&
                    _endpointsLentos.TryRemove(
                        endpoint.Key,
                        out _))
                {
                    removidos++;
                }
            }

            foreach (var trace in _traceIdsComErroTecnico)
            {
                if (trace.Value < dataLimite)
                {
                    _traceIdsComErroTecnico.TryRemove(
                        trace.Key,
                        out _);
                }
            }

            return removidos;
        }

        // Cria uma cópia do incidente enquanto ele está protegido contra
        // alterações concorrentes.
        private static IncidenteErroViewModel CopiarIncidente(
            IncidenteErroViewModel incidente,
            DateTime agora)
        {
            lock (incidente)
            {
                return new IncidenteErroViewModel
                {
                    Identificador =
                        incidente.Identificador,

                    Origem = incidente.Origem,
                    MetodoHttp = incidente.MetodoHttp,
                    Endpoint = incidente.Endpoint,
                    StatusCode = incidente.StatusCode,
                    TipoErro = incidente.TipoErro,
                    CodigoErro = incidente.CodigoErro,
                    Mensagem = incidente.Mensagem,

                    PrimeiraOcorrencia =
                        incidente.PrimeiraOcorrencia,

                    UltimaOcorrencia =
                        incidente.UltimaOcorrencia,

                    QuantidadeOcorrencias =
                        incidente.QuantidadeOcorrencias,

                    DuracaoAtivaSegundos =
                        (incidente.UltimaOcorrencia -
                         incidente.PrimeiraOcorrencia)
                        .TotalSeconds,

                    EstaAtivo =
                        incidente.UltimaOcorrencia >=
                        agora.AddMinutes(
                            -MinutosIncidenteAtivo)
                };
            }
        }

        // Cria a representação do endpoint lento com seus tempos arredondados.
        private static EndpointLentoViewModel CopiarEndpoint(
            EndpointLentoViewModel endpoint)
        {
            lock (endpoint)
            {
                return new EndpointLentoViewModel
                {
                    MetodoHttp = endpoint.MetodoHttp,
                    Endpoint = endpoint.Endpoint,

                    QuantidadeOcorrencias =
                        endpoint.QuantidadeOcorrencias,

                    TempoMedioMs =
                        Math.Round(
                            endpoint.TempoMedioMs,
                            2),

                    MaiorTempoMs =
                        Math.Round(
                            endpoint.MaiorTempoMs,
                            2),

                    UltimoTempoMs =
                        Math.Round(
                            endpoint.UltimoTempoMs,
                            2),

                    UltimaOcorrencia =
                        endpoint.UltimaOcorrencia
                };
            }
        }

        // Gera a chave usada para agrupar ocorrências do mesmo tipo de erro.
        private static string CriarIdentificador(
            ErroMonitoramentoViewModel erro)
        {
            return string.Join(
                ":",
                erro.Origem,
                erro.MetodoHttp ?? "SEM_METODO",
                erro.Endpoint ?? "SEM_ENDPOINT",
                erro.TipoErro,
                erro.CodigoErro ??
                    erro.StatusCode?.ToString() ??
                    "SEM_CODIGO");
        }

        // Procura um código técnico conhecido na exceção e em suas causas.
        private static string? ObterCodigoErro(
            Exception? exception)
        {
            var excecaoAtual = exception;

            while (excecaoAtual is not null)
            {
                foreach (var propriedade in new[]
                {
                    "SqlState",
                    "Code",
                    "ErrorCode"
                })
                {
                    var valor = excecaoAtual
                        .GetType()
                        .GetProperty(propriedade)
                        ?.GetValue(excecaoAtual)
                        ?.ToString();

                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        return valor;
                    }
                }

                excecaoAtual = excecaoAtual.InnerException;
            }

            return exception is null
                ? null
                : $"0x{exception.HResult:X8}";
        }

        // Obtém somente a primeira exceção interna para exibição no painel.
        private static string? ObterExcecaoInterna(
            Exception? exception)
        {
            if (exception?.InnerException is null)
            {
                return null;
            }

            var interna = exception.InnerException;

            return SanitizarMensagem(
                $"{interna.GetType().Name}: {interna.Message}");
        }

        // Remove possíveis dados sensíveis e limita o tamanho da mensagem.
        private static string SanitizarMensagem(
            string? mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
            {
                return "Erro sem mensagem.";
            }

            var mensagemSanitizada = Regex.Replace(
                mensagem,
                @"(?i)(password|pwd|token|secret|authorization)\s*[=:]\s*[^;,\s]+",
                "$1=***");

            mensagemSanitizada = Regex.Replace(
                mensagemSanitizada,
                @"(?i)Bearer\s+[A-Za-z0-9\-\._~\+\/]+=*",
                "Bearer ***");

            const int tamanhoMaximo = 1000;

            if (mensagemSanitizada.Length > tamanhoMaximo)
            {
                mensagemSanitizada =
                    mensagemSanitizada[..tamanhoMaximo];
            }

            return mensagemSanitizada;
        }

        // Aplica o valor padrão quando o texto recebido estiver vazio.
        private static string NormalizarValor(
            string? valor,
            string valorPadrao)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? valorPadrao
                : valor.Trim();
        }

        // Normaliza textos opcionais sem criar valores artificiais.
        private static string? NormalizarValorOuNulo(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        // Reinicia a contagem no intervalo configurado e protege a operação
        // para que apenas uma requisição realize o reset.
        private void ResetarContadorRequisicoesSeNecessario()
        {
            var agora = DateTime.UtcNow;

            if (agora < _inicioContagemRequisicoes
                .AddMinutes(MinutosResetContadorRequisicoes))
            {
                return;
            }

            lock (_bloqueioContadorRequisicoes)
            {
                if (agora < _inicioContagemRequisicoes
                    .AddMinutes(MinutosResetContadorRequisicoes))
                {
                    return;
                }

                Interlocked.Exchange(ref _totalRequisicoes, 0);

                _inicioContagemRequisicoes = agora;
            }
        }
    }
}
