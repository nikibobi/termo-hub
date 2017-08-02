using System.Threading.Tasks;
using TermoHub.Options;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Options;

namespace TermoHub.Services
{
    public class MailKitEmailService : IEmailSender
    {
        private readonly EmailOptions options;

        public MailKitEmailService(IOptions<EmailOptions> options)
        {
            this.options = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(options.SenderName, options.SenderEmail));
            mimeMessage.To.Add(new MailboxAddress(string.Empty, email));
            mimeMessage.Subject = options.SubjectPrefix + subject;

            mimeMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = message,
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(options.SmtpHost, options.SmtpPort, useSsl: false);    
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(options.SenderEmail, options.EmailPassword);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(quit: true);
            }
        }      
    }
}
