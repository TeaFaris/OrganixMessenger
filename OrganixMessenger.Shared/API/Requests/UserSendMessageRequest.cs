namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class UserSendMessageRequest
    {
        public string Text { get; init; }

        public List<FileDTO> Files { get; init; }

        public int? MessageReplyId { get; init; }
    }
}
