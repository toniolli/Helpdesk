using Application.Dto.EmailNotification;
using Application.Dto.EmailNotificationDto;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena a criação, consulta e atualização do estado das notificações
    // de e-mail relacionadas aos chamados.
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailNotificationRepository _emailNotificationRepository;
        private readonly IMapper _mapper;

        public EmailNotificationService(
            IEmailNotificationRepository emailNotificationRepository,
            IMapper mapper)
        {
            _emailNotificationRepository = emailNotificationRepository;
            _mapper = mapper;
        }

        // CRIAÇÃO

        // Cria a notificação inicialmente pendente para processamento do envio.
        public async Task<EmailNotificationResponseDto> Adicionar(
            EmailNotificationRequestDto request)
        {
            var emailNotification = new EmailNotification(
                request.ChamadoId,
                request.Tipo,
                request.Destinatario,
                request.Assunto);

            await _emailNotificationRepository.Adicionar(emailNotification);

            return _mapper.Map<EmailNotificationResponseDto>(emailNotification);
        }

        // CONSULTAS

        public async Task<EmailNotificationResponseDto?> BuscarPorId(int id)
        {
            var emailNotification =
                await _emailNotificationRepository.BuscarPorId(id);

            if (emailNotification is null)
                return null;

            return _mapper.Map<EmailNotificationResponseDto>(emailNotification);
        }

        // ATUALIZAÇÃO DE STATUS

        // Confirma que o e-mail foi enviado com sucesso.
        public async Task AtualizarStatusEnviado(int emailNotificationId)
        {
            var emailNotification =
                await _emailNotificationRepository.BuscarPorId(emailNotificationId);

            if (emailNotification is null)
                throw new DomainExceptionValidation(
                    "Notificação de email não encontrada.");

            emailNotification.MarcarComoEnviado();

            await _emailNotificationRepository.Atualizar(emailNotification);
        }

        // Registra a falha apresentada durante a tentativa de envio.
        public async Task AtualizarStatusFalhou(
            int emailNotificationId,
            string mensagemErro)
        {
            var emailNotification =
                await _emailNotificationRepository.BuscarPorId(emailNotificationId);

            if (emailNotification is null)
                throw new DomainExceptionValidation(
                    "Notificação de email não encontrada.");

            emailNotification.RegistrarFalha(mensagemErro);

            await _emailNotificationRepository.Atualizar(emailNotification);
        }

        // Cancela uma notificação que não deve mais ser processada.
        public async Task Cancelar(int emailNotificationId)
        {
            var emailNotification =
                await _emailNotificationRepository.BuscarPorId(emailNotificationId);

            if (emailNotification is null)
                throw new DomainExceptionValidation(
                    "Notificação de email não encontrada.");

            emailNotification.Cancelar();

            await _emailNotificationRepository.Atualizar(emailNotification);
        }
    }
}
