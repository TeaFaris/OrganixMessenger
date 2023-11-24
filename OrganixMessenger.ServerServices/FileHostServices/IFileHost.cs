namespace OrganixMessenger.ServerServices.FileHostServices
{
    public interface IFileHost
    {
        long MaxFileSize { get; init; }

        Task<UploadedFile> UploadAsync(Stream fileStream, string fileName, string contentType);

        Task<Stream> DownloadAsync(UploadedFile file);

        Task DeleteAsync(UploadedFile file);
    }
}
