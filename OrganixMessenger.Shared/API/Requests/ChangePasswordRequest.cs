namespace OrganixMessenger.Shared.API.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        public string Code { get; init; }

        [Required]
        public string Password { get; init; }
    }
}
