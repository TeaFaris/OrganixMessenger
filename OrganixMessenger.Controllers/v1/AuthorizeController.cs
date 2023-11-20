namespace OrganixMessenger.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    public sealed class AuthorizeController(
                IUserAuthenticationManager authenticationManager,
                IJWTTokenGenerator jwtTokenGenerator,
                ILogger<AuthorizeController> logger
            ) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> Register(RegisterRequest registerRequest)
        {
            string emailDomain = registerRequest.EmailAddress.Split('@')[1];
            if (emailDomain != Request.Host.Value)
            {
                logger.LogInformation(
                        "Guest with ip {ip} failed to register due to not wrong email domain {emailDomain}",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        emailDomain
                    );

                return Responses.BadRequest($"Registration is possible only using the @{Request.Host.Value} email.");
            }

            var result = await authenticationManager.Register(registerRequest.Username, registerRequest.EmailAddress, registerRequest.Password, Role.User);

            if (!result.Successful)
            {
                logger.LogInformation("Guest with ip {ip} failed to register", Request.HttpContext.Connection.RemoteIpAddress);
                return BadRequest(result.Errors);
            }

            logger.LogInformation(
                    "Guest with ip {ip} registered successfully with email {email} and username {username}",
                    Request.HttpContext.Connection.RemoteIpAddress,
                    result.User.Email,
                    result.User.Username
                );

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest loginRequest)
        {
            var user = await authenticationManager.ValidateCredentials(loginRequest.Username, loginRequest.Password);

            if (user is null)
            {
                logger.LogInformation(
                        "Guest with ip {ip} failed to log in to account with username {username} due to invalid credentials",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        loginRequest.Username
                    );

                return Responses.Unauthorized("Invalid credentials.");
            }

            if (!user.EmailConfirmed)
            {
                logger.LogInformation(
                        "Guest with ip {ip} failed to log in to account with username {username} due to unconfirmed email",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        loginRequest.Username
                    );

                return Responses.Unauthorized("You must confirm your email before you log in.");
            }

            var tokens = await jwtTokenGenerator.GenerateTokens(user);

            SetCookiesRefreshToken(tokens.RefreshToken);

            logger.LogInformation(
                        "Guest with ip {ip} successfully logged to in account with username {username}",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        loginRequest.Username
                    );

            return new LoginResponse
            {
                JWTToken = tokens.JWTToken,
                RefreshToken = tokens.RefreshToken
            };
        }

        [HttpPost]
        public async Task<ActionResult<JWTTokens>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (refreshToken is null)
            {
                return Responses.Unauthorized("No 'refreshToken' provided in cookies.");
            }

            var generatedTokens = await jwtTokenGenerator.VerifyAndGenerateTokens(refreshToken);

            if (generatedTokens is null)
            {
                logger.LogInformation(
                        "Guest with ip {ip} provided invalid refresh token",
                        Request.HttpContext.Connection.RemoteIpAddress
                    );

                return Responses.Unauthorized("Invalid refresh token.");
            }

            SetCookiesRefreshToken(generatedTokens.RefreshToken);

            logger.LogInformation(
                        "Guest with ip {ip} successfully refreshed their token",
                        Request.HttpContext.Connection.RemoteIpAddress
                    );

            return generatedTokens;
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            var forgotPasswordResult = await authenticationManager.ForgotPassword(request.Email);

            if (!forgotPasswordResult.Successful)
            {
                logger.LogInformation(
                        "Guest with ip {ip} unsuccessfully requested to reset password for account with email {email}",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        request.Email
                    );

                return Responses.BadRequest(forgotPasswordResult.ErrorMessage!);
            }

            logger.LogInformation(
                        "Guest with ip {ip} requested to reset password for account with email {email}",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        request.Email
                    );

            return Ok();
        }

        private void SetCookiesRefreshToken(string refreshToken)
        {
            var cookiesOption = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMonths(1)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookiesOption);
        }
    }
}