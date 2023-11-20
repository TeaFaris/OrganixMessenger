namespace OrganixMessenger.Shared.API.Requests
{
    public class ConfirmEmailRequest
    {
        [Required]
        public string Code { get; init; }
    }
}
