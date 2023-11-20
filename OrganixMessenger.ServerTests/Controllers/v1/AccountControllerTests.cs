using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrganixMessenger.Controllers.v1;
using OrganixMessenger.Shared.API.Requests;
using System.Net;

namespace OrganixMessenger.ServerTests.Controllers.v1
{
    public class AccountControllerTests
    {
        private const string Ip = "127.0.0.1";

        private readonly Mock<IUserAuthenticationManager> authenticationManager;
        private readonly AccountController controller;

        public AccountControllerTests()
        {
            authenticationManager = new Mock<IUserAuthenticationManager>();
            var logger = new Mock<ILogger<AccountController>>();
            controller = new AccountController(authenticationManager.Object, logger.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = IPAddress.Parse(Ip) },
                    Request = { Host = new HostString("test.com") }
                }
            };
        }

        [Fact]
        public async Task ConfirmEmail_ShouldReturnOk_WhenCodeIsValid()
        {
            // Arrange
            var code = "123456";
            var user = new ApplicationUser { Username = "test", Email = "test@test.com" };
            var result = new ConfirmEmailResult { Successful = true, User = user };
            authenticationManager.Setup(m => m.ConfirmEmailAsync(code)).ReturnsAsync(result);

            // Act
            var response = await controller.ConfirmEmail(new ConfirmEmailRequest { Code = code });

            // Assert
            Assert.IsType<OkResult>(response);
            authenticationManager.Verify(m => m.ConfirmEmailAsync(code), Times.Once);
        }

        [Fact]
        public async Task ConfirmEmail_ShouldReturnBadRequest_WhenCodeIsInvalid()
        {
            // Arrange
            var code = "123456";
            var result = new ConfirmEmailResult { Successful = false };
            authenticationManager.Setup(m => m.ConfirmEmailAsync(code)).ReturnsAsync(result);

            // Act
            var response = await controller.ConfirmEmail(new ConfirmEmailRequest { Code = code });

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
            authenticationManager.Verify(m => m.ConfirmEmailAsync(code), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnOk_WhenCodeIsValid()
        {
            // Arrange
            var code = "123456";
            var password = "password";
            var user = new ApplicationUser { Username = "test", Email = "test@test.com" };
            var result = new ChangePasswordResult { Successful = true, User = user };
            authenticationManager.Setup(m => m.ChangePasswordAsync(code, password)).ReturnsAsync(result);

            // Act
            var response = await controller.ChangePassword(new ChangePasswordRequest { Code = code, Password = password });

            // Assert
            Assert.IsType<OkResult>(response);
            authenticationManager.Verify(m => m.ChangePasswordAsync(code, password), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenCodeIsInvalid()
        {
            // Arrange
            var code = "123456";
            var password = "password";
            var user = new ApplicationUser { Username = "test", Email = "test@test.com" };
            var result = new ChangePasswordResult { Successful = false, User = user };
            authenticationManager.Setup(m => m.ChangePasswordAsync(code, password)).ReturnsAsync(result);

            // Act
            var response = await controller.ChangePassword(new ChangePasswordRequest { Code = code, Password = password });

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
            authenticationManager.Verify(m => m.ChangePasswordAsync(code, password), Times.Once);
        }
    }
}
