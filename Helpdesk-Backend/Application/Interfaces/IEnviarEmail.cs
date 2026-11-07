using Application.Dto.EmailNotificationDto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEnviarEmail
    {
       Task EnviarEmail(EnviarEmailDto enviarEmailDto);
       EnviarEmailDto CriarEmailAberturaChamado(string destinatario,string nomeSolicitante,string numeroChamado);
       EnviarEmailDto CriarEmailAtribuicaoChamado(string destinatario,string nomeTecnico,string numeroChamado);
       EnviarEmailDto CriarEmailEncerramentoChamado(string destinatario,string nomeSolicitante,string numeroChamado);
       EnviarEmailDto CriarEmailRecuperacaoSenha(string destinatario,string nomeUsuario,string senhaTemporaria);

    }
}
