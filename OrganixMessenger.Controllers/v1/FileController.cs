using OrganixMessenger.ServerServices.FileHostServices;
using OrganixMessenger.ServerServices.Repositories.FileRepositories;

namespace OrganixMessenger.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public sealed class FileController(IFileHost fileHost, IFileRepository fileRepository, ILogger<FileController> logger) : ControllerBase
    {
        [SwaggerResponse(HttpStatusCode.OK, typeof(FileStreamResult), Description = "The file as a FileStream")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MessageResponse), Description = "File not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "One or more validation errors occurred")]
        [ReDocCodeSamples]
        [HttpGet("{id}")]
        public async Task<ActionResult> Download(Guid id)
        {
            var file = await fileRepository.GetAsync(id);

            if (file is null)
            {
                logger.LogInformation(
                        "Guest with ip {ip} tried to download non-existent file with id {id}",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        id
                    );

                return Responses.NotFound("File not found.");
            }

            logger.LogInformation(
                        "Guest with ip {ip} successfully downloaded file with id {id}",
                        Request.HttpContext.Connection.RemoteIpAddress,
                        id
                    );

            var fileStream = await fileHost.DownloadAsync(file);

            return File(fileStream, file.ContentType, file.FileName);
        }
    }
}
