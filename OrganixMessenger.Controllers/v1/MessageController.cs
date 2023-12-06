using System.ComponentModel.DataAnnotations;

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
            IMessageRepository messageRepository,
            IFileRepository fileRepository
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

        [ReDocCodeSamples]
        [HttpPost("send")]
        public async Task<ActionResult<MessageDTO>> Post(UserSendMessageRequest message)
        {
            if (string.IsNullOrEmpty(message.Text) && message.Files is null or { Count: 0 })
            {
                return ValidationProblem("Text and files can't be empty or null.");
            }

            string senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var files = new List<UploadedFile>(message.Files.Count);

            foreach (var file in message.Files)
            {
                var serverFile = await fileRepository.GetAsync(file.Id);

                if(serverFile is null)
                {
                    return ValidationProblem($"File with id '{file.Id}' doesn't exists.");
                }

                files.Add(serverFile);
            }

            var serverMessage = new Message
            {
                Files = files,
                SenderId = Guid.Parse(senderId),
                SendTime = DateTime.UtcNow,
                MessageReplyId = message.MessageReplyId,
                Text = message.Text
            };

            await messageRepository.AddAsync(serverMessage);
            await messageRepository.SaveAsync();

            serverMessage = (await messageRepository.GetAsync(serverMessage.Id))!;

            MessageDTO messageDTO = serverMessage.ToDTO();

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
                        (searchRequest.SenderUsername == null || x.Sender.Username.ToLower().Contains(searchRequest.SenderUsername.ToLower()))
                    );

            var foundMessageDTOs = foundMessages
                .Select(x => x.ToDTO());

            return new(foundMessageDTOs);
        }

        /// <summary>
        /// Edits the message with the specified id.
        /// </summary>
        /// <param name="editedMessage">Edited message.</param>
        [SwaggerResponse(HttpStatusCode.OK, typeof(MessageDTO), Description = "The edited message.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "Message was not found or was deleted.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MessageResponse), Description = "You don't have permission to edit this message.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MessageResponse), Description = "File with id '{file.Id}' doesn't exists.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "Text and files can't be empty or null.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Your request was not authorized to access the requested resource. This could be due to missing or invalid authentication credentials.")]
        [ReDocCodeSamples]
        [HttpPut]
        public async Task<ActionResult<MessageDTO>> Edit(EditMessageRequest editedMessage)
        {
            var messageToEdit = await messageRepository.GetAsync(editedMessage.Id);

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

            var newFiles = new List<UploadedFile>(editedMessage.Files.Count);

            foreach (var file in editedMessage.Files)
            {
                var serverFile = await fileRepository.GetAsync(file.Id);

                if (serverFile is null)
                {
                    return Responses.BadRequest($"File with id '{file.Id}' doesn't exists.");
                }

                newFiles.Add(serverFile);
            }

            messageToEdit.MessageReplyId = editedMessage.MessageReplyId;
            messageToEdit.MessageReply = null;
            messageToEdit.Files = newFiles;
            messageToEdit.Text = editedMessage.Text;
            messageToEdit.Edited = true;

            await messageRepository.UpdateAsync(messageToEdit);
            await messageRepository.SaveAsync();

            messageToEdit = (await messageRepository.GetAsync(messageToEdit.Id))!;

            MessageDTO messageDTO = messageToEdit.ToDTO();

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
        public async Task<ActionResult> Delete(int id)
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

            return Ok();
        }
    }
}
