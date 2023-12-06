using OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel;

namespace OrganixMessenger.ServerServices.JWTTokenGeneratorServices
{
    public sealed class JWTTokenGenerator(
                IOptions<JWTSettings> jwtSettings,
                IRefreshTokenRepository refreshTokenRepository,
                IUserRepository userRepository
            ) : IJWTTokenGenerator
    {
        private static readonly JwtSecurityTokenHandler SecurityTokenHandler = new();

        readonly JWTSettings jwtSettings = jwtSettings.Value;

        public async Task<JWTTokens?> VerifyAndGenerateTokens(string refreshToken)
        {
            var storedRefreshToken = await refreshTokenRepository.FindFirstOrDefaultAsync(x => x.Token == refreshToken);

            bool expired = DateTime.UtcNow > storedRefreshToken?.ExpiryDate;

            if (storedRefreshToken is null || expired)
            {
                if (expired)
                {
                    await refreshTokenRepository.RemoveAsync(storedRefreshToken!);
                    await refreshTokenRepository.SaveAsync();
                }

                return null;
            }

            await refreshTokenRepository.RemoveAsync(storedRefreshToken);
            await refreshTokenRepository.SaveAsync();

            var user = (await userRepository.GetAsync(storedRefreshToken.UserId))!;

            return await GenerateTokens(user);
        }

        public async Task<JWTTokens> GenerateTokens(ApplicationUser applicationUser)
        {
            var securityToken = GenerateSecurityToken(applicationUser);

            var refreshToken = await GenerateRefreshToken(applicationUser, securityToken);
            var jwtToken = SecurityTokenHandler.WriteToken(securityToken);

            return new JWTTokens
            {
                JWTToken = jwtToken,
                RefreshToken = refreshToken
            };
        }

        private async Task<string> GenerateRefreshToken(ApplicationUser user, JwtSecurityToken token)
        {
            var refreshToken = new RefreshToken
            {
                JWTId = token.Id,
                Token = Guid.NewGuid().ToString(),
                IssuedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(1),
                User = user,
                UserId = user.Id
            };

            await refreshTokenRepository.AddAsync(refreshToken);
            await refreshTokenRepository.SaveAsync();

            return refreshToken.Token;
        }

        private JwtSecurityToken GenerateSecurityToken(ApplicationUser applicationUser)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            Claim[] claims = 
            [
                new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                new Claim(ClaimTypes.Name, applicationUser.Username),
                new Claim(ClaimTypes.Email, applicationUser.Email),
                new Claim(ClaimTypes.Role, applicationUser.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];

            var token = new JwtSecurityToken(
                    jwtSettings.Issuer,
                    jwtSettings.Audience,
                    claims,
                    expires: DateTime.UtcNow.Add(jwtSettings.ExpiryTime),
                    signingCredentials: credentials
                );

            return token;
        }
    }
}
