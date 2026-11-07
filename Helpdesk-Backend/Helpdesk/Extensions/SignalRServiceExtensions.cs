using Application.Interfaces;
using Helpdesk.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Extensions
{
    // Mantém o registro do serviço de tempo real isolado da configuração principal da aplicação.
    public static class SignalRServiceExtensions
    {
        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            // Publica fisicamente no Hub os eventos recuperados do Outbox.
            services.AddScoped<
                IPublicadorSignalRService,
                PublicadorSignalRService>();

            return services;
        }
    }
}
