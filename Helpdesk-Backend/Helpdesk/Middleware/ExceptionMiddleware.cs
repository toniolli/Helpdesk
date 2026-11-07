using Domain.Validation;
using FluentValidation;
using System.Text.Json;

namespace Helpdesk.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next,ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Erro de validação na requisição.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = StatusCodes.Status400BadRequest,

                    errors = ex.Errors
                        .Select(x => x.ErrorMessage)
                        .ToList(),

                    timestamp = DateTime.UtcNow,

                    traceId = context.TraceIdentifier
                });
            }
            catch (DomainExceptionValidation ex)
            {
                _logger.LogWarning(
                    ex,
                    "Regra de negócio violada: {Message}",
                    ex.Message);

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    ex.Message);
            }
            catch(ConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Conflito de concorrencia:{Message}", ex.Message);

                await HandleExceptionAsync(context, StatusCodes.Status409Conflict, ex.Message);
                    

            }
            catch (AutenticacaoException ex)
            {
                _logger.LogWarning(
                    "Falha de autenticação: {Message}",
                    ex.Message);

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }
            catch (AcessoNegadoException ex)
            {
                _logger.LogWarning(
                    "Acesso negado ao recurso: {Message}",
                    ex.Message);

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro interno não tratado.");

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno.");
            }

        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            int statusCode,
            string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    statusCode,

                    error = message,

                    timestamp = DateTime.UtcNow,

                    traceId = context.TraceIdentifier
                }));
        }
    }
}
//retira os Try e catch dos controllers
//e logger deixa o erro no console para indentificar dps
//Padronização de retorno de erro
