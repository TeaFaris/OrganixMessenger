namespace OrganixMessenger.Shared.API.Responses
{
    public sealed class LoginResponse
    {
        public string JWTToken { get; init; }
        public string RefreshToken { get; init; }
    }
}
