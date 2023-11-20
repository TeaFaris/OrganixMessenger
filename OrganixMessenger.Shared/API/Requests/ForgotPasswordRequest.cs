namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; }
    }
}
