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
            IUserRepository userRepository,
            IFileRepository fileRepository,
            IFileHost fileHost,
            ILogger<AccountController> logger
        ) : ControllerBase
    {

        /// <summary>
        /// Confirms the email address of a user.
        /// </summary>
        /// <param name="confirmEmailRequest">The ConfirmEmailRequest object containing the confirm code.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "Successfully confirmed email")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MessageResponse), Description = "Code is invalid")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred")]
        [ReDocCodeSamples]
        [HttpPost]
        public async Task<ActionResult> ConfirmEmail(ConfirmEmailRequest confirmEmailRequest)
        {
            var result = await authenticationManager.ConfirmEmailAsync(confirmEmailRequest.Code);

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
        /// <param name="changePasswordRequest">The code and password to reset password.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "Successfully changed password")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MessageResponse), Description = "Code is invalid")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred")]
        [ReDocCodeSamples]
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            var result = await authenticationManager.ChangePasswordAsync(changePasswordRequest.Code, changePasswordRequest.Password);

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

        /// <summary>
        /// Changes the profile picture of the user.
        /// </summary>
        /// <param name="profilePicture">The profile picture image file to upload.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "Profile picture successfully changed.")]
        [SwaggerResponse(HttpStatusCode.UnsupportedMediaType, typeof(MessageResponse), Description = "Unsupported media type for profile picture. Only 'image/png' and 'image/jpeg' are supported.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(MessageResponse), Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> ChangeProfilePicture(ChangeProfilePictureRequest profilePicture)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = (await userRepository.GetAsync(Guid.Parse(userId)))!;

            var profilePictureFile =
                await fileHost.UploadAsync(
                        profilePicture.File.OpenReadStream(),
                        profilePicture.File.FileName,
                        profilePicture.File.ContentType
                    );

            await fileRepository.AddAsync(profilePictureFile);
            user.ProfilePicture = profilePictureFile;

            await fileRepository.SaveAsync();
            await userRepository.SaveAsync();

            return Ok();
        }
    }
}
