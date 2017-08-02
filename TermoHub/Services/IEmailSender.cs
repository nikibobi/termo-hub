using System.Threading.Tasks;

namespace TermoHub.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}