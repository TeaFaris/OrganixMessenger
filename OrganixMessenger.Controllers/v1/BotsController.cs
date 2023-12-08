namespace OrganixMessenger.Controllers.v1
{
    /// <summary>
    /// Endpoint for managing Organix Bots.
    /// </summary>
    [OpenApiTag("Bots Endpoint", Description = "Endpoint for managing Organix Bots.")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("IP")]
    [ApiVersion("1.0")]
    public sealed class BotsController(
            ILogger<BotsController> logger,
            IBotRepository botRepository,
            IBotCommandRepository commandRepository,
            IAPITokenGeneratorService apiTokenGenerator,
            IFileRepository fileRepository,
            IFileHost fileHost
        ) : ControllerBase
    {
        /// <summary>
        /// Gets all bots.
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<BotDTO>), Description = "Returns the bot with the specified id")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BotDTO>>> GetAll()
        {
            var serverBots = await botRepository.GetAllAsync();

            var bots = serverBots
                .Where(x => !x.Removed)
                .Select(x => x.ToDTO());

            return new(bots);
        }

        /// <summary>
        /// Gets a bot by its id.
        /// </summary>
        /// <param name="id">The id of the bot.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(BotDTO), Description = "Returns the bot with the specified id.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "This bot doesn't exist.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpGet("{id}")]
        public async Task<ActionResult<BotDTO>> GetById(Guid id)
        {
            var bot = await botRepository.GetAsync(id);

            if (bot is null or { Removed: true })
            {
                return Responses.NotFound("This bot doesn't exist.");
            }

            return bot.ToDTO();
        }

        /// <summary>
        /// Creates a new Organix Bot with the given name and assigns it to the current user.
        /// </summary>
        /// <param name="name">The name of the bot to create.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(BotDTO), Description = "Returns the newly created Organix Bot")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPost("create")]
        public async Task<ActionResult<BotDTO>> Create(
                        [FromQuery]
                        [StringLength(24, MinimumLength = 3)]
                        string name
                    )
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var newBot = new ApplicationBot
            {
                Name = name,
                OwnerId = Guid.Parse(userId),
                Token = apiTokenGenerator.GenerateAPIToken(),
                LastOnline = DateTime.UtcNow
            };

            await botRepository.AddAsync(newBot);
            await botRepository.SaveAsync();

            newBot = (await botRepository.GetAsync(newBot.Id))!;

            return newBot.ToDTO();
        }

        /// <summary>
        /// Generates a new token and deletes the old one.
        /// </summary>
        /// <param name="id">The id of the bot.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(BotDTO), Description = "Returns the newly created Bot Token.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPost("{id}/generatetoken")]
        public async Task<ActionResult<GenerateTokenResponse>> GenerateToken(Guid id)
        {
            var bot = await botRepository.GetAsync(id);

            if (bot is null or { Removed: true })
            {
                return Responses.NotFound("Bot was not found.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (bot.OwnerId != Guid.Parse(userId))
            {
                return Responses.Forbidden("You do not have permission to edit this bot.");
            }

            bot.Token = apiTokenGenerator.GenerateAPIToken();

            await botRepository.UpdateAsync(bot);
            await botRepository.SaveAsync();

            return new GenerateTokenResponse
            {
                Token = apiTokenGenerator.GenerateAPIToken()
            };
        }

        /// <summary>
        /// Changes the profile picture of the specified bot.
        /// </summary>
        /// <param name="id">The id of the bot.</param>
        /// <param name="profilePicture">The profile picture to be set.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "The profile picture is successfully changed.")]
        [SwaggerResponse(HttpStatusCode.UnsupportedMediaType, typeof(MessageResponse), Description = "File 'profilePicture' is not a valid image. Image must be .png or .jpeg.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Bot was not found.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You do not have permission to edit this bot.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPost("{id}/changeprofilepicture")]
        public async Task<ActionResult> ChangeProfilePicture(Guid id, [FromForm] IFormFile profilePicture)
        {
            var bot = await botRepository.GetAsync(id);

            if (bot is null or { Removed: true })
            {
                return Responses.NotFound("Bot was not found.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (bot.OwnerId != Guid.Parse(userId))
            {
                return Responses.Forbidden("You do not have permission to edit this bot.");
            }

            if (profilePicture.ContentType is not "image/png" and not "image/jpeg")
            {
                return Responses.UnsupportedMediaType("File 'profilePicture' is not a valid image. Image must be .png or .jpeg");
            }

            var profilePictureFile = await fileHost
                .UploadAsync(
                    profilePicture.OpenReadStream(),
                    profilePicture.FileName,
                    profilePicture.ContentType
                );

            await fileRepository.AddAsync(profilePictureFile);
            bot.ProfilePicture = profilePictureFile;

            await fileRepository.SaveAsync();
            await botRepository.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Changes the name of the bot with the given id.
        /// </summary>
        /// <param name="id">The id of the bot.</param>
        /// <param name="name">The new name of the bot.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(BotDTO), Description = "Returns the updated bot.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Bot was not found.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You do not have permission to edit this bot.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPost("{id}/changename")]
        public async Task<ActionResult<BotDTO>> ChangeName(
                        Guid id,
                        [FromQuery]
                        [StringLength(24, MinimumLength = 3)]
                        string name
                    )
        {
            var bot = await botRepository.GetAsync(id);

            if (bot is null or { Removed: true })
            {
                return Responses.NotFound("Bot was not found.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (bot.OwnerId != Guid.Parse(userId))
            {
                return Responses.Forbidden("You do not have permission to edit this bot.");
            }

            bot.Name = name;

            await botRepository.UpdateAsync(bot);
            await botRepository.SaveAsync();

            return bot.ToDTO();
        }

        /// <summary>
        /// Deletes a bot with the specified id.
        /// </summary>
        /// <param name="id">The id of the bot to delete.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "The bot was successfully deleted.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Bot was not found.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You do not have permission to edit this bot.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Remove(Guid id)
        {
            var bot = await botRepository.GetAsync(id);

            if (bot is null)
            {
                return Responses.NotFound("Bot with id '{id}' is not found.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (bot.OwnerId != Guid.Parse(userId))
            {
                return Responses.Forbidden("You do not have permission to edit this bot.");
            }

            bot.Removed = true;

            await botRepository.UpdateAsync(bot);
            await botRepository.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Adds a new command to the specified bot.
        /// </summary>
        /// <param name="id">The id of the bot.</param>
        /// <param name="command">The command to add.</param>
        /// <returns>The newly created command.</returns>
        [SwaggerResponse(HttpStatusCode.OK, typeof(BotCommandDTO), Description = "Returns the newly created command.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Bot was not found.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You do not have permission to edit this bot.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPost("{id}/command")]
        public async Task<ActionResult<BotCommandDTO>> AddCommand(Guid id, BotCommandDTO command)
        {
            var bot = await botRepository.GetAsync(id);

            if (bot is null or { Removed: true })
            {
                return Responses.NotFound("Bot was not found.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (bot.OwnerId != Guid.Parse(userId))
            {
                return Responses.Forbidden("You do not have permission to edit this bot.");
            }

            var newCommand = new BotCommand
            {
                Name = command.Name,
                Trigger = command.Trigger,
                Description = command.Description,
                BotId = id
            };

            await commandRepository.AddAsync(newCommand);
            await commandRepository.SaveAsync();

            newCommand = (await commandRepository.GetAsync(newCommand.Id))!;

            return newCommand.ToDTO();
        }

        /// <summary>
        /// Edits a command for a given bot.
        /// </summary>
        /// <param name="id">The id of the bot.</param>
        /// <param name="commandId">The id of the command.</param>
        /// <param name="command">The command to edit.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(BotCommandDTO), Description = "Returns the edited command.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Bot was not found.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Command was not found.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You do not have permission to edit this bot.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPut("{id}/command/{commandId}")]
        public async Task<ActionResult<BotCommandDTO>> EditCommand(Guid id, int commandId, [FromBody] EditBotCommandRequest command)
        {
            var bot = await botRepository.GetAsync(id);
            var commandToEdit = await commandRepository.GetAsync(commandId);

            if (bot is null or { Removed: true })
            {
                return Responses.NotFound("Bot was not found.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (bot.OwnerId != Guid.Parse(userId))
            {
                return Responses.Forbidden("You do not have permission to edit this bot.");
            }

            if (commandToEdit is null)
            {
                return Responses.NotFound("Command was not found.");
            }

            commandToEdit.Trigger = command.Trigger ?? commandToEdit.Trigger;
            commandToEdit.Name = command.Name ?? commandToEdit.Name;
            commandToEdit.Description = command.Description ?? commandToEdit.Description;

            await commandRepository.UpdateAsync(commandToEdit);
            await commandRepository.SaveAsync();

            commandToEdit = (await commandRepository.GetAsync(commandId))!;

            return commandToEdit.ToDTO();
        }

        /// <summary>
        /// Removes a command from a bot.
        /// </summary>
        /// <param name="id">The id of the bot.</param>
        /// <param name="commandId">The id of the command.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "Command was successfully removed.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Bot was not found.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Command was not found.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You do not have permission to edit this bot.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpDelete("{id}/command/{commandId}")]
        public async Task<ActionResult> RemoveCommand(Guid id, int commandId)
        {
            var bot = await botRepository.GetAsync(id);
            var command = await commandRepository.GetAsync(commandId);

            if (bot is null or { Removed: true })
            {
                return Responses.NotFound("Bot was not found.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (bot.OwnerId != Guid.Parse(userId))
            {
                return Responses.Forbidden("You do not have permission to edit this bot.");
            }

            if (command is null)
            {
                return Responses.NotFound("Command was not found.");
            }

            await commandRepository.RemoveAsync(command);
            await commandRepository.SaveAsync();

            return Ok();
        }
    }
}
