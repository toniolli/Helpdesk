using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEmailNotificationRepository
    {


        Task<EmailNotification> Adicionar (EmailNotification emailNotification);
        Task<EmailNotification?> BuscarPorId(int id);
        Task<EmailNotification> Atualizar(EmailNotification emailNotification);

    }
}
