namespace OrganixMessenger.Controllers.v1
{
    /// <summary>
    /// Endpoint for retrieve information about users.
    /// </summary>
    [OpenApiTag("User Endpoint", Description = "Endpoint for retrieve information about users.")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("IP")]
    [ApiVersion("1.0")]
    public sealed class UserController(IUserRepository userRepository) : ControllerBase
    {
        /// <summary>
        /// Gets all users.
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, typeof(UserDTO), Description = "Returns all users.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var users = userRepository.GetAllAsync();

            return Ok(users);
        }

        /// <summary>
        /// Gets the user with the specified id.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(UserDTO), Description = "Returns the user with the specified id")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "This user doesn't exist")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetById(Guid id)
        {
            var user = await userRepository.GetAsync(id);

            if (user is null)
            {
                return Responses.NotFound("This user doesn't exist.");
            }

            return user.ToDTO();
        }
    }
}
