namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class ChangeProfilePictureRequest
    {
        [Required]
        public IFormFile File { get; init; }

        [Match("image/png", "image/jpeg")]
        public string ContentType => File.ContentType;
    }
}
