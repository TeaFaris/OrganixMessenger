namespace OrganixMessenger.Shared.API.Responses
{
    public class MessageResponse(object message)
    {
        [Required]
        public object Message { get; set; } = message;
    }
}
