using Application.Interfaces;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics;

namespace Helpdesk.Middleware
{
    public class MonitoramentoRequisicaoMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IMonitoramentoService
            _monitoramentoService;

        public MonitoramentoRequisicaoMiddleware(
            RequestDelegate next,
            IMonitoramentoService monitoramentoService)
        {
            _next = next;
            _monitoramentoService = monitoramentoService;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            var cronometro = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                cronometro.Stop();

                var rota = ObterRota(context);

                try
                {
                    await _monitoramentoService
                        .RegistrarRequisicao(
                            context.Request.Method,
                            rota,
                            context.Response.StatusCode,
                            cronometro.Elapsed.TotalMilliseconds,
                            context.TraceIdentifier);
                }
                catch
                {
                    // Uma falha no monitoramento não pode
                    // interromper a requisição principal.
                }
            }
        }

        private static string ObterRota(HttpContext context)
        {
            var rotaMapeada =
                (context.GetEndpoint() as RouteEndpoint)?
                .RoutePattern
                .RawText;

            if (!string.IsNullOrWhiteSpace(rotaMapeada))
            {
                return rotaMapeada;
            }

            return context.Request.Path.Value
                ?? "Endpoint não identificado";
        }
    }
}