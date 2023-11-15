namespace OrganixMessenger.ServerTests.Services
{
    public class JWTTokenGeneratorTests
    {
        private readonly JWTTokenGenerator tokenGenerator;
        private readonly IOptions<JWTSettings> mockJWTSettings;
        private readonly Mock<IRefreshTokenRepository> mockRefreshTokenRepository;
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly ApplicationUser testUser;

        public JWTTokenGeneratorTests()
        {
            // Arrange
            var jwtSettings = new JWTSettings
            {
                Key = "your_secret_128bit_key_your_secret_128bit_key_your_secret_128bit_key",
                Issuer = "your_issuer",
                Audience = "your_audience",
                ExpiryTime = TimeSpan.FromMinutes(15)
            };

            mockJWTSettings = new OptionsWrapper<JWTSettings>(jwtSettings);
            mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
            mockUserRepository = new Mock<IUserRepository>();
            tokenGenerator = new JWTTokenGenerator(mockJWTSettings, mockRefreshTokenRepository.Object, mockUserRepository.Object);
            testUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "testuser@example.com",
                Role = Role.User
            };
        }

        [Fact]
        public async Task GenerateTokens_Should_Return_Valid_JWT_And_Refresh_Token()
        {
            // Act
            var tokens = await tokenGenerator.GenerateTokens(testUser);

            // Assert
            Assert.NotNull(tokens);
            Assert.NotNull(tokens.JWTToken);
            Assert.NotNull(tokens.RefreshToken);

            // Validate JWT token
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mockJWTSettings.Value.Key));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = mockJWTSettings.Value.Issuer,
                ValidAudience = mockJWTSettings.Value.Audience,
                IssuerSigningKey = securityKey
            };
            var principal = new JwtSecurityTokenHandler().ValidateToken(tokens.JWTToken, validationParameters, out var validatedToken);
            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
            Assert.Equal(testUser.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal(testUser.Username, principal.FindFirst(ClaimTypes.Name)?.Value);
            Assert.Equal(testUser.Email, principal.FindFirst(ClaimTypes.Email)?.Value);
            Assert.Equal(testUser.Role.ToString(), principal.FindFirst(ClaimTypes.Role)?.Value);
        }

        [Fact]
        public async Task VerifyAndGenerateTokens_Should_Return_New_Tokens_If_Refresh_Token_Is_Valid()
        {
            // Arrange
            RefreshToken oldRefreshToken = null!;

            mockRefreshTokenRepository.Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token => oldRefreshToken = token);
            mockUserRepository.Setup(x => x.GetAsync(testUser.Id))
                .ReturnsAsync(testUser);

            var oldTokens = await tokenGenerator.GenerateTokens(testUser);

            mockRefreshTokenRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(new[] { oldRefreshToken });

            // Act
            var newTokens = await tokenGenerator.VerifyAndGenerateTokens(oldTokens.RefreshToken);

            // Assert
            Assert.NotNull(newTokens);
            Assert.NotNull(newTokens.JWTToken);
            Assert.NotNull(newTokens.RefreshToken);
            Assert.NotEqual(oldTokens.JWTToken, newTokens.JWTToken);
            Assert.NotEqual(oldTokens.RefreshToken, newTokens.RefreshToken);
        }

        [Fact]
        public async Task VerifyAndGenerateTokens_Should_Return_Null_If_Refresh_Token_Is_Not_Found()
        {
            // Arrange
            var invalidRefreshToken = "invalid_token";

            // Act
            var tokens = await tokenGenerator.VerifyAndGenerateTokens(invalidRefreshToken);

            // Assert
            Assert.Null(tokens);
        }

        [Fact]
        public async Task VerifyAndGenerateTokens_Should_Return_Null_If_Refresh_Token_Is_Expired()
        {
            // Arrange
            RefreshToken oldRefreshToken = null!;

            mockRefreshTokenRepository.Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token => oldRefreshToken = token);
            mockUserRepository.Setup(x => x.GetAsync(testUser.Id))
                .ReturnsAsync(testUser);

            var oldTokens = await tokenGenerator.GenerateTokens(testUser);

            mockRefreshTokenRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(new[] { oldRefreshToken });

            var expiredRefreshToken = (await mockRefreshTokenRepository.Object.FindFirstOrDefaultAsync(x => x.Token == oldTokens.RefreshToken))!;
            expiredRefreshToken.ExpiryDate = DateTime.UtcNow.AddDays(-1); // Set expiry date to yesterday

            // Act
            var tokens = await tokenGenerator.VerifyAndGenerateTokens(oldTokens.RefreshToken);

            // Assert
            Assert.Null(tokens);
        }
    }
}
