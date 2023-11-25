using Microsoft.AspNetCore.RateLimiting;
﻿using OrganixMessenger.ServerServices.UserAuthenticationManagerServices;

namespace OrganixMessenger.Controllers.v1
{
    /// <summary>
    /// Endpoint for user account related operations.
    /// </summary>
    [OpenApiTag("Account Endpoint", Description = "Endpoint for user account related operations.")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [EnableRateLimiting("IP")]
    [ApiVersion("1.0")]
    public class AccountController(
            IUserAuthenticationManager authenticationManager,
            ILogger<AccountController> logger
        ) : ControllerBase
    {

        /// <summary>
        /// Confirms the email address of a user.
        /// </summary>
        /// <param name="requestData">The ConfirmEmailRequest object containing the confirm code.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "Successfully confirmed email")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MessageResponse), Description = "Code is invalid")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred")]
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

        /// <summary>
        /// Changes password of the user.
        /// </summary>
        /// <param name="requestData">The code and password to reset password.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "Successfully changed password")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MessageResponse), Description = "Code is invalid")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred")]
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
