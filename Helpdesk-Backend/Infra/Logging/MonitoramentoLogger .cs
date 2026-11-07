using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infra.Logging
{
    internal class MonitoramentoLogger : ILogger
    {
        private readonly string _categoria;
        private readonly IMonitoramentoService
            _monitoramentoService;
        private readonly IHttpContextAccessor
            _httpContextAccessor;

        public MonitoramentoLogger(
            string categoria,
            IMonitoramentoService monitoramentoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _categoria = categoria;
            _monitoramentoService = monitoramentoService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IDisposable? BeginScope<TState>(
            TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Error;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            try
            {
                var mensagem = formatter(
                    state,
                    exception);

                var contexto =
                    _httpContextAccessor.HttpContext;

                var metodoHttp =
                    contexto?.Request.Method;

                var endpoint =
                    contexto?.Request.Path.Value;

                var traceId =
                    contexto?.TraceIdentifier;

                int? statusCode = null;

                if (contexto is not null)
                {
                    statusCode =
                        contexto.Response.StatusCode >= 400
                            ? contexto.Response.StatusCode
                            : exception is not null
                                ? StatusCodes
                                    .Status500InternalServerError
                                : null;
                }

                _monitoramentoService
                    .RegistrarErro(
                        _categoria,
                        mensagem,
                        exception,
                        metodoHttp,
                        endpoint,
                        statusCode,
                        traceId)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                // O monitoramento nunca pode interromper
                // o funcionamento principal da aplicação.
            }
        }
    }
}