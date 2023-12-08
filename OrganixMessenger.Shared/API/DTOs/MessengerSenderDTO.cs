namespace OrganixMessenger.Shared.API.DTOs
{
    public sealed class MessengerSenderDTO
    {
        public Guid Id { get; init; }

        public bool IsBot { get; init; }
    }
}
