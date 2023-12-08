namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class BotSendMessageRequest
    {
        [StringLength(32, MinimumLength = 5)]
        public string? CustomUsername { get; init; }

        public Guid? CustomProfilePictureId { get; init; }

        public string? Text { get; init; }

        public List<Guid>? FileIds { get; init; }

        public int? MessageReplyId { get; init; }
    }
}
