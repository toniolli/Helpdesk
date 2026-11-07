namespace Helpdesk.Middleware
{
    public class TrocaSenhaObrigatoriaMiddleware
    {
        private readonly RequestDelegate _next;

        public TrocaSenhaObrigatoriaMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            var usuarioAutenticado =
                context.User.Identity?.IsAuthenticated == true;

            var deveTrocarSenha =
                context.User.HasClaim(
                    "DeveTrocarSenha",
                    "true");

            var rotaAlterarSenha =
                context.Request.Path.Equals(
                    new PathString(
                        "/api/Usuario/alterar-senha"));

            if (usuarioAutenticado &&
                deveTrocarSenha &&
                !rotaAlterarSenha)
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                context.Response.ContentType =
                    "application/json";

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        error =
                            "É necessário alterar a senha antes de acessar o sistema."
                    });

                return;
            }

            await _next(context);
        }
    }
}