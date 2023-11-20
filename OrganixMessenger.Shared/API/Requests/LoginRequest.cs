namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class LoginRequest
    {
        [Required]
        public string Username { get; init; }

        [Required]
        public string Password { get; init; }
    }
}
