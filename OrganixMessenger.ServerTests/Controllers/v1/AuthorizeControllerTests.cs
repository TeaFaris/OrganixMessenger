using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrganixMessenger.Controllers.v1;
using OrganixMessenger.ServerTests.Mocks;
using OrganixMessenger.Shared.API.Requests;
using OrganixMessenger.Shared.API.Responses;
using System.Net;

namespace OrganixMessenger.ServerTests.Controllers.v1
{
    public class AuthorizeControllerTests
    {
        private const string RefreshTokenValue = "refresh-token";
        private const string RefreshTokenKey = "refreshToken";
        private const string Ip = "127.0.0.1";
        private readonly AuthorizeController controller;
        private readonly Mock<IUserAuthenticationManager> authenticationManagerMock;
        private readonly Mock<IJWTTokenGenerator> jwtTokenGeneratorMock;

        public AuthorizeControllerTests()
        {
            authenticationManagerMock = new Mock<IUserAuthenticationManager>();
            jwtTokenGeneratorMock = new Mock<IJWTTokenGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizeController>>();
            controller = new AuthorizeController(authenticationManagerMock.Object, jwtTokenGeneratorMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                EmailAddress = "testuser@test.com",
                Password = "test123"
            };
            var registerResult = new RegisterUserResult
            {
                Successful = true,
                User = new ApplicationUser
                {
                    Username = "testuser",
                    Email = "testuser@test.com",
                    Role = Role.User
                }
            };
            authenticationManagerMock.Setup(m => m.Register(registerRequest.Username, registerRequest.EmailAddress, registerRequest.Password, Role.User))
                .ReturnsAsync(registerResult);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) },
                    Request = { Host = new HostString("test.com") }
                }
            };

            // Act
            var result = await controller.Register(registerRequest);

            // Assert
            Assert.IsType<OkResult>(result);
            authenticationManagerMock.Verify(m => m.Register(registerRequest.Username, registerRequest.EmailAddress, registerRequest.Password, Role.User), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenRegistrationIsUnsuccessful()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                EmailAddress = "testuser@test.com",
                Password = "test123"
            };
            var registerResult = new RegisterUserResult
            {
                Successful = false,
                Errors = new[] { "Username already exists." }
            };
            authenticationManagerMock.Setup(m => m.Register(registerRequest.Username, registerRequest.EmailAddress, registerRequest.Password, Role.User))
                .ReturnsAsync(registerResult);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) },
                    Request = { Host = new HostString("test.com") }
                }
            };

            // Act
            var result = await controller.Register(registerRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal(registerResult.Errors, badRequestResult.Value);
            authenticationManagerMock.Verify(m => m.Register(registerRequest.Username, registerRequest.EmailAddress, registerRequest.Password, Role.User), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailDomainIsWrong()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                EmailAddress = "testuser@gmail.com",
                Password = "test123"
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) },
                    Request = { Host = new HostString("test.com") }
                }
            };

            // Act
            var result = await controller.Register(registerRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var msgResponse = badRequestResult.Value as MessageResponse;
            Assert.Equal("Registration is possible only using the @test.com email.", msgResponse.Message);
            authenticationManagerMock.Verify(m => m.Register(registerRequest.Username, registerRequest.EmailAddress, registerRequest.Password, Role.User), Times.Never);
        }

        [Fact]
        public async Task Login_ShouldReturnLoginResponse_WhenCredentialsAreValidAndEmailIsConfirmed()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "test123"
            };
            var user = new ApplicationUser
            {
                Username = "testuser",
                Email = "testuser@test.com",
                Role = Role.User,
                EmailConfirmed = true
            };
            var tokens = new JWTTokens
            {
                JWTToken = "jwt-token",
                RefreshToken = RefreshTokenValue
            };
            authenticationManagerMock.Setup(m => m.ValidateCredentials(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync(user);
            jwtTokenGeneratorMock.Setup(m => m.GenerateTokens(user))
                .ReturnsAsync(tokens);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) }
                }
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            Assert.Null(result.Result);
            var loginResponse = result.Value!;
            Assert.Equal(tokens.JWTToken, loginResponse.JWTToken);
            Assert.Equal(tokens.RefreshToken, loginResponse.RefreshToken);
            authenticationManagerMock.Verify(m => m.ValidateCredentials(loginRequest.Username, loginRequest.Password), Times.Once);
            jwtTokenGeneratorMock.Verify(m => m.GenerateTokens(user), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "test123"
            };
            authenticationManagerMock.Setup(m => m.ValidateCredentials(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync((ApplicationUser)null);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) }
                }
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            var msgResponse = unauthorizedResult.Value as MessageResponse;
            Assert.Equal("Invalid credentials.", msgResponse.Message);
            authenticationManagerMock.Verify(m => m.ValidateCredentials(loginRequest.Username, loginRequest.Password), Times.Once);
            jwtTokenGeneratorMock.Verify(m => m.GenerateTokens(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenEmailIsUnconfirmed()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "test123"
            };
            var user = new ApplicationUser
            {
                Username = "testuser",
                Email = "testuser@test.com",
                Role = Role.User,
                EmailConfirmed = false
            };
            authenticationManagerMock.Setup(m => m.ValidateCredentials(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync(user);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) }
                }
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            var msgResponse = unauthorizedResult.Value as MessageResponse;
            Assert.Equal("You must confirm your email before you log in.", msgResponse.Message);
            authenticationManagerMock.Verify(m => m.ValidateCredentials(loginRequest.Username, loginRequest.Password), Times.Once);
            jwtTokenGeneratorMock.Verify(m => m.GenerateTokens(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnJWTTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var tokens = new JWTTokens
            {
                JWTToken = "jwt-token",
                RefreshToken = "new-refresh-token"
            };
            jwtTokenGeneratorMock.Setup(m => m.VerifyAndGenerateTokens(RefreshTokenValue))
                .ReturnsAsync(tokens);

            var cookieContainer = new RequestCookieCollection
            {
                { RefreshTokenKey, RefreshTokenValue }
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) },
                    Request = { Cookies = cookieContainer }
                }
            };

            // Act
            var result = await controller.RefreshToken();

            // Assert
            Assert.Null(result.Result);
            Assert.NotNull(result.Value);
            var jwtTokens = result.Value;
            Assert.Equal(tokens.JWTToken, jwtTokens.JWTToken);
            Assert.Equal(tokens.RefreshToken, jwtTokens.RefreshToken);
            jwtTokenGeneratorMock.Verify(m => m.VerifyAndGenerateTokens(RefreshTokenValue), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var refreshToken = RefreshTokenValue;
            jwtTokenGeneratorMock.Setup(m => m.VerifyAndGenerateTokens(RefreshTokenValue))
                .ReturnsAsync((JWTTokens)null);

            var cookieContainer = new RequestCookieCollection
            {
                { RefreshTokenKey, RefreshTokenValue }
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) },
                    Request = { Cookies = cookieContainer }
                }
            };

            // Act
            var result = await controller.RefreshToken();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            var msgResponse = unauthorizedResult.Value as MessageResponse;
            Assert.Equal("Invalid refresh token.", msgResponse.Message);
            jwtTokenGeneratorMock.Verify(m => m.VerifyAndGenerateTokens(RefreshTokenValue), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenIsMissing()
        {
            // Arrange
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) }
                }
            };

            // Act
            var result = await controller.RefreshToken();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            var msgResponse = unauthorizedResult.Value as MessageResponse;
            Assert.Equal("No 'refreshToken' provided in cookies.", msgResponse.Message);
            jwtTokenGeneratorMock.Verify(m => m.VerifyAndGenerateTokens(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPassword_WhenEmailIsValidAndResetSuccessful_ReturnsOk()
        {
            // Arrange
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) }
                }
            };

            var email = "test@example.com";
            var request = new ForgotPasswordRequest { Email = email };
            var result = new ForgotPasswordResult { Successful = true };
            authenticationManagerMock.Setup(x => x.ForgotPassword(email)).ReturnsAsync(result);

            // Act
            var actionResult = await controller.ForgotPassword(request);

            // Assert
            Assert.IsType<OkResult>(actionResult);
            authenticationManagerMock.Verify(x => x.ForgotPassword(email), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_WhenEmailIsValidAndResetUnsuccessful_ReturnsBadRequest()
        {
            // Arrange
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) }
                }
            };

            var email = "test@example.com";
            var request = new ForgotPasswordRequest { Email = email };
            var result = new ForgotPasswordResult { Successful = false, ErrorMessage = "Some error" };
            authenticationManagerMock.Setup(x => x.ForgotPassword(email)).ReturnsAsync(result);

            // Act
            var actionResult = await controller.ForgotPassword(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult);
            var badRequestObjectResult = actionResult as BadRequestObjectResult;
            var msgResponse = badRequestObjectResult.Value as MessageResponse;
            Assert.Equal(result.ErrorMessage, msgResponse.Message);
            authenticationManagerMock.Verify(x => x.ForgotPassword(email), Times.Once);
        }
    }
}