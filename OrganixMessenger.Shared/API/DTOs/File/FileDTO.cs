namespace OrganixMessenger.Shared.API.DTOs.File
{
    public sealed class FileDTO
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        public string FileName { get; init; }

        [Required]
        public string ContentType { get; init; }
    }
}
