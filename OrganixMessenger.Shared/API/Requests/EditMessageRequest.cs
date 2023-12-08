namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class EditMessageRequest
    {
        public string? Text { get; init; }

        public List<FileDTO>? Files { get; init; }

        public int? MessageReplyId { get; init; }
    }
}
