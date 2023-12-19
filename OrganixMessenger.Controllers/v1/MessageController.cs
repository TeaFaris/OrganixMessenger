namespace OrganixMessenger.Controllers.v1
{
    /// <summary>
    /// Endpoint for managing messages for client.
    /// </summary>
    [OpenApiTag("Client Messages Endpoint", Description = "Endpoint for managing messages for client.")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("IP")]
    [ApiVersion("1.0")]
    public sealed class MessageController(
            ILogger<MessageController> logger,
            IMessageRepository messageRepository,
            IFileRepository fileRepository,
            IHubContext<MessengerHub, IMessengerHub> messengerHub,
            IHubContext<MessengerBotHub, IMessengerHub> messengerBotHub
        ) : ControllerBase
    {
        /// <summary>
        /// Gets a message by its id.
        /// </summary>
        /// <param name="id">The id of the message.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(MessageDTO), Description = "Returns the message with the specified id.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Message was not found or was deleted.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDTO>> Get(int id)
        {
            var message = await messageRepository.GetAsync(id);

            if (message is null or { Removed: true })
            {
                return Responses.NotFound("Message was not found or was deleted.");
            }

            var dto = message.ToDTO();

            return dto;
        }

        /// <summary>
        /// Gets a slice of messages from the messenger.
        /// </summary>
        /// <param name="slice">The index of the first message to get and the number of messages to get.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<MessageDTO>), Description = "Returns a list of messages.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetSlice([FromQuery] MessageSliceRequest slice)
        {
            var messages = await messageRepository
                .GetSliceOfMessagesAsync(slice.From, slice.Count, x => !x.Removed);

            var messageDTOs = messages.Select(x => x.ToDTO());

            return new(messageDTOs);
        }

        /// <summary>
        /// Sends a message to the messenger.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(MessageDTO), Description = "The message successfully sent.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "Text and files can't be empty or null.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "File with id '{fileId}' doesn't exists.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPost("send")]
        public async Task<ActionResult<MessageDTO>> Post(UserSendMessageRequest message)
        {
            if (string.IsNullOrEmpty(message.Text) && message.FileIds is null or { Count: 0 })
            {
                return ValidationProblem("Text and files can't be empty or null.");
            }

            string senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var files = new List<UploadedFile>(message.FileIds?.Count ?? 0);

            if(message.FileIds is not null)
            {
                foreach (var fileId in message.FileIds)
                {
                    var serverFile = await fileRepository.GetAsync(fileId);

                    if(serverFile is null)
                    {
                        return ValidationProblem($"File with id '{fileId}' doesn't exists.");
                    }

                    files.Add(serverFile);
                }
            }

            var serverMessage = new Message
            {
                Files = files,
                SenderId = Guid.Parse(senderId),
                SendTime = DateTime.UtcNow,
                MessageReplyId = message.MessageReplyId,
                Text = message.Text ?? ""
            };

            await messageRepository.AddAsync(serverMessage);
            await messageRepository.SaveAsync();

            serverMessage = (await messageRepository.GetAsync(serverMessage.Id))!;

            MessageDTO messageDTO = serverMessage.ToDTO();

            await messengerHub.Clients.All.ReceiveMessages(messageDTO);
            await messengerBotHub.Clients.All.ReceiveMessages(messageDTO);

            return messageDTO;
        }

        /// <summary>
        /// Gets a list of messages that matching the specified parameters.
        /// </summary>
        /// <param name="searchRequest">The search parameters.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<MessageDTO>), Description = "Returns a list of messages that matching the specified parameters.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpGet("find")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> Find([FromQuery] SearchMessageRequest searchRequest)
        {
            if (searchRequest is
                {
                    After: null,
                    Before: null,
                    Text: null,
                    SenderUsername: null,
                    SenderBot: null,
                    Edited: null,
                    IsReply: null,
                    WithFiles: null,
                    TextAccuracy: true or false
                })
            {
                return new([]);
            }

            var foundMessages = await messageRepository
                .GetSliceOfMessagesAsync(searchRequest.Page * 200 - 200, 200, x =>
                        (!x.Removed) &&
                        (searchRequest.Text == null || (searchRequest.TextAccuracy ? x.Text == searchRequest.Text : x.Text.ToLower().Contains(searchRequest.Text.ToLower()))) &&
                        (searchRequest.IsReply == null || (searchRequest.IsReply == true ? x.MessageReplyId != null : x.MessageReplyId == null)) &&
                        (searchRequest.WithFiles == null || (searchRequest.WithFiles == true ? x.Files.Count != 0 : x.Files.Count == 0)) &&
                        (searchRequest.Edited == null || (searchRequest.Edited == true ? x.Edited : !x.Edited)) &&
                        (searchRequest.After == null || searchRequest.After > x.SendTime) &&
                        (searchRequest.Before == null || searchRequest.Before < x.SendTime) &&
                        // TODO: Is sender bot
                        (searchRequest.SenderUsername == null || x.Sender.Name.ToLower().Contains(searchRequest.SenderUsername.ToLower()))
                    );

            var foundMessageDTOs = foundMessages
                .Select(x => x.ToDTO());

            return new(foundMessageDTOs);
        }

        /// <summary>
        /// Edits the message with the specified id.
        /// </summary>
        /// <param name="id">The id of the message.</param>
        /// <param name="editedMessage">Edited message.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(MessageDTO), Description = "The edited message.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Message was not found or was deleted.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You don't have permission to edit this message.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MessageResponse), Description = "File with id '{file.Id}' doesn't exists.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "Text and files can't be empty or null.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPut("{id}")]
        public async Task<ActionResult<MessageDTO>> Edit(int id, [FromBody] EditMessageRequest editedMessage)
        {
            var messageToEdit = await messageRepository.GetAsync(id);

            if (messageToEdit is null or { Removed: true })
            {
                return Responses.NotFound("Message was not found or was deleted.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (Guid.Parse(userId) != messageToEdit.Sender.Id)
            {
                return Responses.Forbidden("You don't have permission to edit this message.");
            }

            if (string.IsNullOrEmpty(editedMessage.Text) && editedMessage.Files is null or { Count: 0 })
            {
                return Responses.BadRequest("Text and files can't be empty or null.");
            }

            var newFiles = new List<UploadedFile>(editedMessage.Files?.Count ?? 0);

            if(editedMessage.Files is not null)
            {
                foreach (var file in editedMessage.Files)
                {
                    var serverFile = await fileRepository.GetAsync(file.Id);

                    if (serverFile is null)
                    {
                        return Responses.BadRequest($"File with id '{file.Id}' doesn't exists.");
                    }

                    newFiles.Add(serverFile);
                }
            }

            var previousMessageJson = JsonSerializer.Serialize(messageToEdit.ToDTO());

            messageToEdit.MessageReplyId = editedMessage.MessageReplyId ?? messageToEdit.MessageReplyId;
            messageToEdit.MessageReply = null;
            messageToEdit.Files = newFiles.Count != 0 ? newFiles : messageToEdit.Files;
            messageToEdit.Text = editedMessage.Text ?? messageToEdit.Text;
            messageToEdit.Edited = true;

            logger.LogInformation(
                    "User {username} with ip {ip} and id {id} edited message with id {messageId} from: {previousMessageJson} to: {newMessageJson}",
                    User.FindFirstValue(ClaimTypes.Name)!,
                    Request.HttpContext.Connection.RemoteIpAddress,
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    id,
                    previousMessageJson,
                    JsonSerializer.Serialize(messageToEdit.ToDTO())
                );

            await messageRepository.UpdateAsync(messageToEdit);
            await messageRepository.SaveAsync();

            messageToEdit = (await messageRepository.GetAsync(messageToEdit.Id))!;

            MessageDTO messageDTO = messageToEdit.ToDTO();

            await messengerHub.Clients.All.ReceiveEditedMessages(messageDTO);
            await messengerBotHub.Clients.All.ReceiveEditedMessages(messageDTO);

            return messageDTO;
        }

        /// <summary>
        /// Deletes a message with the specified id.
        /// </summary>
        /// <param name="id">Id of the message to delete.</param>
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "The message was successfully deleted.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Message was not found or was deleted.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You don't have permission to delete this message.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MessageResponse), Description = "File with id '{file.Id}' doesn't exists.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Remove(int id)
        {
            var messageToRemove = await messageRepository.GetAsync(id);

            if (messageToRemove is null or { Removed: true })
            {
                return Responses.NotFound("Message was not found or was deleted.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (Guid.Parse(userId) != messageToRemove.Sender.Id)
            {
                return Responses.Forbidden("You don't have permission to delete this message.");
            }

            messageToRemove.Removed = true;

            await messageRepository.UpdateAsync(messageToRemove);
            await messageRepository.SaveAsync();

            logger.LogInformation(
                    "User {username} with ip {ip} and id {id} removed message with id {messageId}",
                    User.FindFirstValue(ClaimTypes.Name)!,
                    Request.HttpContext.Connection.RemoteIpAddress,
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    id
                );

            await messengerHub.Clients.All.ReceiveRemovedMessages(id);
            await messengerBotHub.Clients.All.ReceiveRemovedMessages(id);

            return Ok();
        }
    }
}
