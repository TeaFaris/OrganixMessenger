using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace OrganixMessenger.ServerServices.EmailServices
{
    public sealed class SmtpEmailSender(IOptions<EmailServiceSettings> emailConfiguration) : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = new MimeMessage();

            mail.From.Add(MailboxAddress.Parse(emailConfiguration.Value.Username));
            mail.To.Add(MailboxAddress.Parse(email));

            mail.Subject = subject;
            mail.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };

            using var smtpClient = new SmtpClient();
            
            await smtpClient.ConnectAsync(emailConfiguration.Value.Host, emailConfiguration.Value.Port, true);
            await smtpClient.AuthenticateAsync(emailConfiguration.Value.Username, emailConfiguration.Value.Password);
            await smtpClient.SendAsync(mail);
            await smtpClient.DisconnectAsync(true);
        }
    }
}
