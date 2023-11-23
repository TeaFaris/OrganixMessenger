namespace OrganixMessenger.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AccountController(
            IUserAuthenticationManager authenticationManager,
            ILogger<AccountController> logger
        ) : ControllerBase
    {
        [ReDocCodeSamples]
        [HttpPost]
        public async Task<ActionResult> ConfirmEmail(ConfirmEmailRequest requestData)
        {
            var result = await authenticationManager.ConfirmEmailAsync(requestData.Code);

            if (!result.Successful)
            {
                logger.LogInformation(
                        "Guest with ip {ip} failed to confirm email due to invalid code",
                        Request.HttpContext.Connection.RemoteIpAddress
                    );

                return Responses.BadRequest("Code is invalid.");
            }

            logger.LogInformation(
                        "Guest with ip {ip} confirmed email for account with username {username} and email {email}",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        result.User.Username,
                        result.User.Email
                    );

            return Ok();
        }

        [ReDocCodeSamples]
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordRequest requestData)
        {
            var result = await authenticationManager.ChangePasswordAsync(requestData.Code, requestData.Password);

            if (!result.Successful)
            {
                logger.LogInformation(
                        "Guest with ip {ip} failed to change password due to invalid code",
                        Request.HttpContext.Connection.RemoteIpAddress
                    );

                return Responses.BadRequest("Code is invalid.");
            }

            logger.LogInformation(
                        "Guest with ip {ip} changed password for account with username {username} and email {email}",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        result.User.Username,
                        result.User.Email
                    );

            return Ok();
        }
    }
}
