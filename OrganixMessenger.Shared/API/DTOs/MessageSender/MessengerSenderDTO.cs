namespace OrganixMessenger.Shared.API.DTOs.MessageSender
{
    public sealed class MessengerSenderDTO
    {
        [Required]
        [Key]
        public Guid Id { get; init; }

        public bool IsBot { get; init; }
    }
}
