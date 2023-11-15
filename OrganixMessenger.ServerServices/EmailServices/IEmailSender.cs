namespace OrganixMessenger.ServerServices.EmailServices
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}