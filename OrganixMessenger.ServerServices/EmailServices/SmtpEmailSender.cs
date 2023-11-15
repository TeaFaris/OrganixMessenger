using Microsoft.Extensions.Options;
using OrganixMessenger.ServerConfigurations;
using System.Net;
using System.Net.Mail;

namespace OrganixMessenger.ServerServices.EmailServices
{
    public sealed class SmtpEmailSender(IOptions<EmailServceSettings> emailConfiguration) : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient(emailConfiguration.Value.Host, emailConfiguration.Value.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(emailConfiguration.Value.Username, emailConfiguration.Value.Password)
            };

            return client.SendMailAsync(new MailMessage(
                        from: emailConfiguration.Value.Username,
                        to: email,
                        subject: subject,
                        body: htmlMessage
                    ));
        }
    }
}
