namespace Application.Dto.Autenticacao
{
    // Resultado interno utilizado entre o serviço e o controller.
    // O RefreshToken deve ser colocado no cookie e nunca no corpo da resposta.
    public class ResultadoAutenticacaoDto
    {
        public AutenticacaoResponseDto Resposta { get; set; } = new();
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiraEmUtc { get; set; }
    }
}