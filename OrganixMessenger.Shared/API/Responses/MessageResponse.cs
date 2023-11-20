namespace OrganixMessenger.Shared.API.Responses
{
    public class MessageResponse(string message)
    {
        [Required]
        public string Message { get; set; } = message;
    }
}
