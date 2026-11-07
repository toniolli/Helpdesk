
using Application.Dto.EmailNotificationDto;
using Application.Interfaces;
using Infra.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.Encodings.Web;

public class EnviarEmailService : IEnviarEmail
{
    private readonly EmailService _settings;

    public EnviarEmailService(IOptions<EmailService> settings)
    {
        _settings = settings.Value;
    }

    
    public async Task EnviarEmail(EnviarEmailDto enviarEmailDto)
    {
        var email = new MimeMessage();

        email.From.Add(
            new MailboxAddress(
                _settings.Remetente,
                _settings.Email));

        email.To.Add(
            MailboxAddress.Parse(
                enviarEmailDto.Destinatario));

        email.Subject = enviarEmailDto.Assunto;

        email.Body = new TextPart("html")
        {
            Text = enviarEmailDto.Corpo
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _settings.Servidor,
            _settings.Porta,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _settings.Email,
            _settings.Senha);

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }

    public EnviarEmailDto CriarEmailAberturaChamado(string destinatario, string nomeSolicitante,string numeroChamado)
    {
         return new EnviarEmailDto
        {
                    Destinatario = destinatario,

                    Assunto = $"Chamado {numeroChamado} registrado com sucesso",

                    Corpo = $@"
                    <h2>Chamado Registrado</h2>
 
                    <p>Olá {nomeSolicitante}.</p>
 
                    <p>Seu chamado foi registrado com sucesso.</p>
 
                    <p><strong>Número:</strong> {numeroChamado}</p>
 
                    <p>Nossa equipe analisará sua solicitação em breve.</p>
 
                    <p>Atenciosamente,<br/>
                    Help Desk</p>"
        };
    }

    public EnviarEmailDto CriarEmailAtribuicaoChamado(string destinatario,string nomeTecnico,string numeroChamado)
    {
        return new EnviarEmailDto
        {
            Destinatario = destinatario,

            Assunto = $"Novo chamado atribuído - {numeroChamado}",

            Corpo = $@"
            <h2>Novo Chamado Atribuído</h2>
 
            <p>Olá {nomeTecnico}.</p>
 
            <p>Um novo chamado foi atribuído para você.</p>
 
            <p><strong>Número:</strong> {numeroChamado}</p>
 
            <p>Acesse o sistema para iniciar o atendimento.</p>
 
            <p>Atenciosamente,<br/>
            Help Desk</p>"
         };
    }

    public EnviarEmailDto CriarEmailEncerramentoChamado(string destinatario,string nomeSolicitante,string numeroChamado)
    {
        return new EnviarEmailDto
        {
                    Destinatario = destinatario,

                    Assunto = $"Chamado {numeroChamado} encerrado",

                    Corpo = $@"
                    <h2>Chamado Encerrado</h2>
 
                    <p>Olá {nomeSolicitante}.</p>
 
                    <p>Seu chamado foi encerrado.</p>
 
                    <p><strong>Número:</strong> {numeroChamado}</p>
 
                    <p>Caso ainda exista alguma pendência, entre em contato com a equipe de suporte.</p>
 
                    <p>Atenciosamente,<br/>
                    Help Desk</p>"
        };
    }

    public EnviarEmailDto CriarEmailRecuperacaoSenha(
    string destinatario,
    string nomeUsuario,
    string senhaTemporaria)
    {
        var usuarioSeguro = HtmlEncoder.Default.Encode(nomeUsuario);
        var senhaSegura = HtmlEncoder.Default.Encode(senhaTemporaria);

        return new EnviarEmailDto
        {
            Destinatario = destinatario,

            Assunto = "Recuperação de senha - Help Desk",

            Corpo = $@"
            <h2>Recuperação de senha</h2>

            <p>Olá, {usuarioSeguro}.</p>

            <p>Foi solicitada uma recuperação de senha para sua conta.</p>

            <p>Use a senha temporária abaixo para acessar o sistema:</p>

            <p style=""font-size: 18px;"">
                <strong>{senhaSegura}</strong>
            </p>

            <p>
                Por segurança, será obrigatório criar uma nova senha
                logo após o login.
            </p>

            <p>
                Se você não solicitou esta recuperação, entre em contato
                com o setor de TI.
            </p>

            <p>
                Atenciosamente,<br/>
                Help Desk
            </p>"
        };
    }

}