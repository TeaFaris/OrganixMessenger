using Microsoft.Extensions.Options;
using OrganixMessenger.ServerConfigurations;
using System.Net;
using System.Net.Mail;

namespace OrganixMessenger.ServerServices.EmailServices
{
    public sealed class SmtpEmailSender(IOptions<EmailServceSettings> emailConfiguration) : IEmailSender
    {
        private readonly EmailServceSettings emailConfiguration = emailConfiguration.Value;

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient(emailConfiguration.Host, emailConfiguration.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(emailConfiguration.Username, emailConfiguration.Password)
            };

            return client.SendMailAsync(new MailMessage(
                        from: emailConfiguration.Username,
                        to: email,
                        subject: subject,
                        body: htmlMessage
                    ));
        }
    }
}
