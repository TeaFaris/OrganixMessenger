using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace OrganixMessenger.Controllers.Util
{
    public static class Responses
    {
        public static OkObjectResult Ok(string message)
        {
            return new OkObjectResult(new MessageResponse(message));
        }

        public static BadRequestObjectResult BadRequest(string message)
        {
            return new BadRequestObjectResult(new MessageResponse(message));
        }

        public static UnauthorizedObjectResult Unauthorized(string message)
        {
            return new UnauthorizedObjectResult(new MessageResponse(message));
        }

        public static ObjectResult Forbidden(string message)
        {
            return new ObjectResult(new MessageResponse(message)) { StatusCode = StatusCodes.Status403Forbidden };
        }

        public static NotFoundObjectResult NotFound(string message)
        {
            return new NotFoundObjectResult(new MessageResponse(message));
        }

        public static ConflictObjectResult Conflict(string message)
        {
            return new ConflictObjectResult(new MessageResponse(message));
        }

        public static ObjectResult UnsupportedMediaType(string message)
        {
            return new ObjectResult(new MessageResponse(message)) { StatusCode = StatusCodes.Status415UnsupportedMediaType };
        }

        public static ObjectResult ServiceUnavailable(string message)
        {
            return new ObjectResult(new MessageResponse(message)) { StatusCode = StatusCodes.Status503ServiceUnavailable };
        }

        public static OkObjectResult Ok(object value)
        {
            return Ok(JsonSerializer.Serialize(value));
        }

        public static BadRequestObjectResult BadRequest(object value)
        {
            return BadRequest(JsonSerializer.Serialize(value));
        }

        public static UnauthorizedObjectResult Unauthorized(object value)
        {
            return Unauthorized(JsonSerializer.Serialize(value));
        }

        public static ObjectResult Forbidden(object value)
        {
            return Forbidden(JsonSerializer.Serialize(value));
        }

        public static NotFoundObjectResult NotFound(object value)
        {
            return NotFound(JsonSerializer.Serialize(value));
        }

        public static ConflictObjectResult Conflict(object value)
        {
            return Conflict(JsonSerializer.Serialize(value));
        }

        public static ObjectResult UnsupportedMediaType(object value)
        {
            return UnsupportedMediaType(JsonSerializer.Serialize(value));
        }

        public static ObjectResult ServiceUnavailable(object value)
        {
            return ServiceUnavailable(JsonSerializer.Serialize(value));
        }
    }
}
