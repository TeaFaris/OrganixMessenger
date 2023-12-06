namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class EditMessageRequest
    {
        public int Id { get; init; }

        public string Text { get; init; }

        public List<FileDTO> Files { get; init; }

        public int? MessageReplyId { get; init; }
    }
}
