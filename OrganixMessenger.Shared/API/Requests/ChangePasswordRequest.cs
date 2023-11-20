namespace OrganixMessenger.Shared.API.Requests
{
    public class ChangePasswordRequest
    {
        public string Code { get; init; }
        public string Password { get; init; }
    }
}
