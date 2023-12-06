namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class SearchMessageRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Page { get; init; }

        public string? Text { get; init; }

        public bool TextAccuracy { get; init; }

        public DateTime? Before { get; init; }

        public DateTime? After { get; init; }

        public string? SenderUsername { get; init; }

        public bool? SenderBot { get; init; }

        public bool? WithFiles { get; init; }

        public bool? IsReply { get; init; }

        public bool? Edited { get; init; }
    }
}
