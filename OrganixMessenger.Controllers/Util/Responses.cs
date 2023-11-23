namespace OrganixMessenger.Controllers.Util
{
    public static class Responses
    {
        public static OkObjectResult Ok(object message)
        {
            return new OkObjectResult(new MessageResponse(message));
        }

        public static BadRequestObjectResult BadRequest(object message)
        {
            return new BadRequestObjectResult(new MessageResponse(message));
        }

        public static UnauthorizedObjectResult Unauthorized(object message)
        {
            return new UnauthorizedObjectResult(new MessageResponse(message));
        }

        public static ObjectResult Forbidden(object message)
        {
            return new ObjectResult(new MessageResponse(message)) { StatusCode = StatusCodes.Status403Forbidden };
        }

        public static NotFoundObjectResult NotFound(object message)
        {
            return new NotFoundObjectResult(new MessageResponse(message));
        }

        public static ConflictObjectResult Conflict(object message)
        {
            return new ConflictObjectResult(new MessageResponse(message));
        }

        public static ObjectResult UnsupportedMediaType(object message)
        {
            return new ObjectResult(new MessageResponse(message)) { StatusCode = StatusCodes.Status415UnsupportedMediaType };
        }

        public static ObjectResult ServiceUnavailable(object message)
        {
            return new ObjectResult(new MessageResponse(message)) { StatusCode = StatusCodes.Status503ServiceUnavailable };
        }
    }
}
