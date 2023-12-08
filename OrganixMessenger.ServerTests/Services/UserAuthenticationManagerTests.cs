using OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel;

namespace OrganixMessenger.ServerTests.Services
{
    public class UserAuthenticationManagerTests
    {
        // Arrange
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<IEmailSender> emailSenderMock;
        private readonly Mock<IHttpContextService> httpContextServiceMock;
        private readonly UserAuthenticationManager userAuthenticationManager;

        public UserAuthenticationManagerTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            emailSenderMock = new Mock<IEmailSender>();
            httpContextServiceMock = new Mock<IHttpContextService>();

            httpContextServiceMock.Setup(x => x.GetBaseUrl()).Returns("https://localhost");

            userAuthenticationManager = new UserAuthenticationManager(
                userRepositoryMock.Object,
                emailSenderMock.Object,
                httpContextServiceMock.Object
            );
        }

        [Fact]
        public async Task Register_ShouldReturnSuccessfulResult_WhenUserIsUniqueAndValid()
        {
            // Arrange
            var username = "testuser";
            var email = "testuser@example.com";
            var password = "testpassword";
            var role = Role.User;

            var expectedUser = new ApplicationUser
            {
                Name = username,
                Email = email,
                VereficationToken = It.IsAny<string>(),
                PasswordHash = It.IsAny<string>(),
                Role = role
            };

            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync(Enumerable.Empty<ApplicationUser>());

            userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ApplicationUser>()))
                .Callback<ApplicationUser>(user => expectedUser = user);

            // Act
            var result = await userAuthenticationManager.RegisterAsync(username, email, password, role);

            // Assert
            Assert.True(result.Successful);
            Assert.Equal(expectedUser, result.User);
            Assert.Empty(result.Errors);

            userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()), Times.Exactly(2));
            userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApplicationUser>()), Times.Once);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);

            emailSenderMock.Verify(x => x.SendEmailAsync(
                It.Is<string>(e => e == email),
                It.IsAny<string>(),
                It.Is<string>(m => m.Contains(expectedUser.VereficationToken))
            ), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldReturnErrorResult_WhenUserIsNotUnique()
        {
            // Arrange
            var username = "testuser";
            var email = "testuser@example.com";
            var password = "testpassword";
            var role = Role.User;

            var existingUser = new ApplicationUser
            {
                Name = username,
                Email = email,
                VereficationToken = "some-token",
                PasswordHash = "some-hash",
                Role = role
            };

            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync(new List<ApplicationUser> { existingUser });

            // Act
            var result = await userAuthenticationManager.RegisterAsync(username, email, password, role);

            // Assert
            Assert.False(result.Successful);
            Assert.Null(result.User);
            Assert.Equal(2, result.Errors.Count());
            Assert.Contains("This username is already taken.", result.Errors);
            Assert.Contains("This email is already taken.", result.Errors);

            userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()), Times.Exactly(2));
            userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApplicationUser>()), Times.Never);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Never);

            emailSenderMock.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        [Fact]
        public async Task Register_ShouldReturnErrorResult_WhenUserIsNotValid()
        {
            // Arrange
            var username = "testuser";
            var email = "invalid-email";
            var password = "testpassword";
            var role = Role.User;

            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync(Enumerable.Empty<ApplicationUser>());

            // Act
            var result = await userAuthenticationManager.RegisterAsync(username, email, password, role);

            // Assert
            Assert.False(result.Successful);
            Assert.Null(result.User);
            Assert.Single(result.Errors);
            Assert.Contains("The Email field is not a valid e-mail address.", result.Errors);

            userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()), Times.Exactly(2));
            userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApplicationUser>()), Times.Never);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Never);

            emailSenderMock.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        [Fact]
        public async Task VerifyEmail_ShouldReturnSuccessfulResult_WhenCodeIsValid()
        {
            // Arrange
            var code = "some-token";

            var user = new ApplicationUser
            {
                Name = "testuser",
                Email = "testuser@example.com",
                VereficationToken = code,
                PasswordHash = "some-hash",
                Role = Role.User
            };

            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([ user ]);

            // Act
            var result = await userAuthenticationManager.ConfirmEmailAsync(code);

            // Assert
            Assert.True(result.Successful);

            userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()), Times.Once);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);

            Assert.True(user.EmailConfirmed);
        }

        [Fact]
        public async Task VerifyEmail_ShouldReturnErrorResult_WhenCodeIsInvalid()
        {
            // Arrange
            var code = "some-token";

            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([]);

            // Act
            var result = await userAuthenticationManager.ConfirmEmailAsync(code);

            // Assert
            Assert.False(result.Successful);

            userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()), Times.Once);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task ValidateCredentials_Should_Return_User_If_Valid_Password()
        {
            // Arrange
            var username = "testuser";
            var password = "testpass";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new ApplicationUser
            {
                Name = username,
                PasswordHash = passwordHash
            };
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([ user ]);

            // Act
            var result = await userAuthenticationManager.ValidateCredentialsAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user, result);
        }

        [Fact]
        public async Task ValidateCredentials_Should_Return_Null_If_Invalid_Password()
        {
            // Arrange
            var username = "testuser";
            var password = "testpass";
            var wrongPassword = "wrongpass";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new ApplicationUser
            {
                Name = username,
                PasswordHash = passwordHash
            };
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([ user ]);

            // Act
            var result = await userAuthenticationManager.ValidateCredentialsAsync(username, wrongPassword);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateCredentials_Should_Return_Null_If_No_User_Found()
        {
            // Arrange
            var username = "testuser";
            var password = "testpass";
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([]);

            // Act
            var result = await userAuthenticationManager.ValidateCredentialsAsync(username, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_Success_If_User_Exists_And_Send_Email()
        {
            // Arrange
            var email = "test@test.com";
            var user = new ApplicationUser
            {
                Email = email,
                EmailConfirmed = true
            };
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([ user ]);
            userRepositoryMock.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);
            emailSenderMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            httpContextServiceMock.Setup(x => x.GetBaseUrl()).Returns("http://localhost");

            // Act
            var result = await userAuthenticationManager.ForgotPasswordAsync(email);

            // Assert
            Assert.True(result.Successful);
            Assert.Null(result.ErrorMessage);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
            emailSenderMock.Verify(x => x.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_Failure_If_User_Does_Not_Exist()
        {
            // Arrange
            var email = "test@test.com";
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([]);

            // Act
            var result = await userAuthenticationManager.ForgotPasswordAsync(email);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal("User with this email does not exist.", result.ErrorMessage);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Never);
            emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_Failure_If_User_Already_Requested_Password_Change()
        {
            // Arrange
            var email = "test@test.com";
            var user = new ApplicationUser
            {
                Email = email,
                EmailConfirmed = true,
                PasswordResetToken = "some-token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(10)
            };
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([ user ]);

            // Act
            var result = await userAuthenticationManager.ForgotPasswordAsync(email);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal("You already requested password change. Wait until it expires before requesting a new one.", result.ErrorMessage);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Never);
            emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_Failure_If_User_Email_Is_Not_Confirmed()
        {
            // Arrange
            var email = "test@test.com";
            var user = new ApplicationUser
            {
                Email = email,
                EmailConfirmed = false,
                PasswordResetToken = "some-token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(10)
            };
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([ user ]);

            // Act
            var result = await userAuthenticationManager.ForgotPasswordAsync(email);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal("You must confirm your email before you log in.", result.ErrorMessage);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Never);
            emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ChangePassword_Should_Return_Success_If_Valid_Token_And_New_Password()
        {
            // Arrange
            var code = "some-token";
            var newPassword = "newpass";
            var user = new ApplicationUser
            {
                PasswordResetToken = code,
                EmailConfirmed = true,
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(10)
            };
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([ user ]);
            userRepositoryMock.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await userAuthenticationManager.ChangePasswordAsync(code, newPassword);

            // Assert
            Assert.True(result.Successful);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
            Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash));
        }

        [Fact]
        public async Task ChangePassword_Should_Return_Failure_If_Invalid_Token()
        {
            // Arrange
            var code = "some-token";
            var newPassword = "newpass";
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([]);

            // Act
            var result = await userAuthenticationManager.ChangePasswordAsync(code, newPassword);

            // Assert
            Assert.False(result.Successful);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task ChangePassword_Should_Return_Failure_If_Token_Expired()
        {
            // Arrange
            var code = "some-token";
            var newPassword = "newpass";
            var user = new ApplicationUser
            {
                PasswordResetToken = code,
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(-10)
            };
            userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .ReturnsAsync([ user ]);

            // Act
            var result = await userAuthenticationManager.ChangePasswordAsync(code, newPassword);

            // Assert
            Assert.False(result.Successful);
            userRepositoryMock.Verify(x => x.SaveAsync(), Times.Never);
        }
    }
}