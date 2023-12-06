namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class MessageSliceRequest
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int From { get; init; }

        [Required]
        [Range(1, 200)]
        public int Count { get; init; }
    }
}
