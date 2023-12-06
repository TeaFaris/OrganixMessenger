namespace OrganixMessenger.ServerModels.MessageModel
{
    public sealed class Message
    {
        [Key]
        public int Id { get; init; }

        public string Text { get; set; }

        public List<UploadedFile> Files { get; set; }

        [Required]
        public DateTime SendTime { get; init; }

        public int? MessageReplyId { get; set; }
        [ForeignKey(nameof(MessageReplyId))]
        public Message? MessageReply { get; set; }

        public bool Removed { get; set; }
        public bool Edited { get; set; }

        public Guid SenderId { get; init; }
        [ForeignKey(nameof(SenderId))]
        public MessengerEntity Sender { get; init; }
    }
}
