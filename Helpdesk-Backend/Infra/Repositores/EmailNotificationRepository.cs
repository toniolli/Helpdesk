using Domain.Entities;
using Domain.Interfaces;
using Infra.Context;

namespace Infra.Repositores
{
    // Centraliza a persistência e a consulta das notificações de e-mail.
    public class EmailNotificationRepository : IEmailNotificationRepository
    {
        private readonly AppDbContext _emailRepository;

        public EmailNotificationRepository(AppDbContext emailRepository)
        {
            _emailRepository = emailRepository;
        }

        // PERSISTÊNCIA

        public async Task<EmailNotification> Adicionar(
            EmailNotification emailNotification)
        {
            await _emailRepository
                .EmailNotifications
                .AddAsync(emailNotification);

            await _emailRepository.SaveChangesAsync();

            return emailNotification;
        }

        public async Task<EmailNotification> Atualizar(
            EmailNotification emailNotification)
        {
            _emailRepository
                .EmailNotifications
                .Update(emailNotification);

            await _emailRepository.SaveChangesAsync();

            return emailNotification;
        }

        // CONSULTAS

        public async Task<EmailNotification?> BuscarPorId(int id)
        {
            return await _emailRepository
                .EmailNotifications
                .FindAsync(id);
        }
    }
}
