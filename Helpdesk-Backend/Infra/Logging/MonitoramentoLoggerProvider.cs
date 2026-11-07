using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infra.Logging
{
    public class MonitoramentoLoggerProvider : ILoggerProvider
    {
        private readonly IMonitoramentoService
            _monitoramentoService;

        private readonly IHttpContextAccessor
            _httpContextAccessor;

        public MonitoramentoLoggerProvider(
            IMonitoramentoService monitoramentoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _monitoramentoService =
                monitoramentoService;

            _httpContextAccessor =
                httpContextAccessor;
        }

        public ILogger CreateLogger(
            string categoryName)
        {
            return new MonitoramentoLogger(
                categoryName,
                _monitoramentoService,
                _httpContextAccessor);
        }

        public void Dispose()
        {
        }
    }
}