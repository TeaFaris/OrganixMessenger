
namespace OrganixMessenger.ServerServices.JWTTokenGeneratorService
{
    public interface IJWTTokenGenerator
    {
        Task<JWTTokens> GenerateTokens(ApplicationUser applicationUser);
        Task<JWTTokens?> VerifyAndGenerateTokens(string refreshToken);
    }

    public sealed class JWTTokens
    {
        public string JWTToken { get; init; }
        public string RefreshToken { get; init; }
    }
}