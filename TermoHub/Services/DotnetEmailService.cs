using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using TermoHub.Options;

namespace TermoHub.Services
{
    public class DotnetEmailService : IEmailSender
    {
        private readonly EmailOptions options;

        public DotnetEmailService(IOptions<EmailOptions> options)
        {
            this.options = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var client = new SmtpClient(options.SmtpHost, options.SmtpPort))
            {
                client.Credentials = new NetworkCredential(options.SenderEmail, options.EmailPassword);
                var from = new MailAddress(options.SenderEmail, options.SenderName);
                var to = new MailAddress(email);
                var mailMessage = new MailMessage(from, to)
                {
                    Subject = options.SubjectPrefix + subject,
                    Body = message,
                    IsBodyHtml = true
                };
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
