using Application.Dto.Comentario;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAtualizacaoTempoRealService
    {
        // Avisa um usuário específico de que sua lista e seu contador de notificações mudaram.
        Task NotificarNotificacaoAtualizadaAsync(string usuarioID);

        // Avisa o outro participante do chamado de que um comentário foi criado.
        // O autor não recebe o próprio evento; o frontend recebe os IDs e consulta os dados atualizados.
        Task NotificarComentarioCriadoAsync(Chamado chamado,string autorId, ComentarioResponseDto comentario);

        // Avisa todos os usuários conectados ao grupo da fila de que um chamado entrou ou saiu dela.
        Task NotificarFilaAtualizadaAsync(int chamadoId);

        // Avisa usuários específicos de que seus indicadores individuais do dashboard mudaram.
        Task NotificarDashboardAtualizadoAsync(IEnumerable<string> usuarioIds);
        //Avisa um conjunto de usuario por notificação, para gerar apenas um evento no banco 
        Task NotificarNotificacoesAtualizadasAsync(IEnumerable<string> usuarioIds);

        // Avisa os participantes de um chamado de que seus dados ou status foram alterados.
        Task NotificarChamadoAtualizadoAsync(IEnumerable<string> usuarioIds,int chamadoId);

        // Avisa o grupo gerencial de que os indicadores globais do dashboard mudaram.
        Task NotificarDashboardGlobalAtualizadoAsync();


    }
}
