using Application.Dto.EmailNotification;
using Application.Dto.EmailNotificationDto;

namespace Application.Interfaces
{
    public interface IEmailNotificationService
    {
        Task<EmailNotificationResponseDto> Adicionar(EmailNotificationRequestDto request);
        Task<EmailNotificationResponseDto?> BuscarPorId(int id);
        Task AtualizarStatusEnviado(int emailNotificationId);
        Task AtualizarStatusFalhou(int emailNotificationId,string mensagemErro);
        Task Cancelar(int emailNotificationId);
    }
}