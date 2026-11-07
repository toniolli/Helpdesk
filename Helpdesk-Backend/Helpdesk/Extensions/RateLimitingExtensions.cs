using System.Threading.RateLimiting;

namespace Helpdesk.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddLoginRateLimiting(
            this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddPolicy("login", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString()
                        ?? "ip-desconhecido";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ip,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode =
                        StatusCodes.Status429TooManyRequests;

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new
                        {
                            error =
                                "Muitas tentativas. Aguarde um minuto e tente novamente."
                        },
                        cancellationToken);
                };
            });

            return services;
        }
    }
}