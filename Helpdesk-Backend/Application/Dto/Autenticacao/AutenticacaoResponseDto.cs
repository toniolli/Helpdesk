namespace Application.Dto.Autenticacao
{
    // Dados de autenticação que podem ser devolvidos ao frontend.
    // O refresh token não aparece aqui porque será enviado em cookie HttpOnly.
    public class AutenticacaoResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiraEmUtc { get; set; }
        public bool ModoPainel { get; set; }
    }
}