using OrganixMessenger.Shared.API.DTOs.MessageSender;

namespace OrganixMessenger.Shared.API.DTOs.Message
{
    public sealed class MessageDTO
    {
        [Key]
        [Required]
        public int Id { get; init; }

        public string Text { get; init; }

        [Required]
        public DateTime SendTime { get; init; }

        [Required]
        public MessengerSenderDTO Sender { get; init; }

        public List<FileDTO> Files { get; init; }

        public int? MessageReplyId { get; init; }
        public bool Edited { get; init; }
    }
}
