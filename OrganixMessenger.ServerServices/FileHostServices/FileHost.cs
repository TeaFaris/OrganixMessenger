namespace OrganixMessenger.ServerServices.FileHostServices
{
    public sealed class FileHost(ILogger<FileHost> logger) : IFileHost
    {
        public long MaxFileSize { get; init; } = 1024 * 1024 * 100;

        private static readonly string UploadsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        public async Task<UploadedFile> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            if (!Directory.Exists(UploadsDirectoryPath))
            {
                Directory.CreateDirectory(UploadsDirectoryPath);
            }

            string trustedFileNameForFileStorage;
            var untrustedFileName = fileName;
            var trustedFileNameForDisplay =
                WebUtility.HtmlEncode(untrustedFileName);

            if (fileStream.Length == 0)
            {
                logger.LogInformation("{FileName} length is 0.",
                    trustedFileNameForDisplay);

                throw new IOException("File size is 0.");
            }
            else if (fileStream.Length > MaxFileSize)
            {
                logger.LogInformation("{FileName} of {Length} bytes is " +
                    "larger than the limit of {Limit} bytes.",
                    trustedFileNameForDisplay, fileStream.Length, MaxFileSize);

                throw new IOException("File size larger than the limit.");
            }

            try
            {
                trustedFileNameForFileStorage = Path.GetRandomFileName();
                var path = Path.Combine(
                        UploadsDirectoryPath,
                        trustedFileNameForFileStorage
                    );

                await using FileStream fs = new(path, FileMode.Create);
                await fileStream.CopyToAsync(fs);

                logger.LogInformation("{FileName} saved at {Path}",
                    trustedFileNameForDisplay, path);
            }
            catch (IOException ex)
            {
                logger.LogError("{FileName} error on upload: {Message}",
                    trustedFileNameForDisplay, ex.Message);
                throw;
            }

            var uploadResult = new UploadedFile
            {
                FileName = untrustedFileName,
                StoredFileName = trustedFileNameForFileStorage,
                ContentType = contentType,
            };

            return uploadResult;
        }

        public async Task<Stream> DownloadAsync(UploadedFile file)
        {
            var pathToFile = Path.Combine(
                    UploadsDirectoryPath,
                    file.StoredFileName
                );

            var ms = new MemoryStream();

            await using var fileStream = new FileStream(pathToFile, FileMode.Open);
            await fileStream.CopyToAsync(ms);

            ms.Position = 0;

            return ms;
        }

        public Task DeleteAsync(UploadedFile file)
        {
            var pathToFile = Path.Combine(
                    UploadsDirectoryPath,
                    file.StoredFileName
                );

            if (!File.Exists(pathToFile))
            {
                logger.LogError("File '{FileName}' on path '{pathToFile}' doesn't exist.", file.FileName, pathToFile);
                throw new IOException($"File '{file.FileName}' on path '{pathToFile}' doesn't exist.");
            }

            try
            {
                File.Delete(pathToFile);
            }
            catch (IOException ex)
            {
                logger.LogError("{FileName} error on delete: {Message}", file.FileName, ex.Message);
                throw;
            }

            return Task.CompletedTask;
        }
    }
}