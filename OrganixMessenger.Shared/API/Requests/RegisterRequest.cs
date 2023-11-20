namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class RegisterRequest
    {
        [Required]
        [StringLength(32, MinimumLength = 5)]
        public string Username { get; init; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; init; }

        [Required]
        [StringLength(32, MinimumLength = 6)]
        public string Password { get; init; }
    }
}
