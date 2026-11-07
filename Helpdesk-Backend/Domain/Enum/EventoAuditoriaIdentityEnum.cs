namespace Domain.Enum
{
    public enum EventoAuditoriaIdentityEnum
    {
        LoginSucesso = 1,
        LoginFalha = 2,
        LoginBloqueado = 3,
        CriacaoUsuario = 4,
        AlteracaoUsuario = 5,
        AtivacaoUsuario = 6,
        DesativacaoUsuario = 7,
        ExclusaoUsuario = 8,
        AlteracaoSenha = 9,
        RedefinicaoSenha = 10,
        RenovacaoSessaoSucesso = 11,
        RenovacaoSessaoFalha = 12,
        Logout = 13,
        RevogacaoSessoes = 14,
        ReutilizacaoRefreshToken = 15
    }
}