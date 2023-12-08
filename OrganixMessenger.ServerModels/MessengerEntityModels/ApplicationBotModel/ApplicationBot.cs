namespace OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel
{
    public sealed class ApplicationBot : MessengerEntity
    {
        [Required]
        public string Token { get; set; }

        public Guid OwnerId { get; init; }
        [ForeignKey(nameof(OwnerId))]
        public ApplicationUser Owner { get; init; }

        public List<BotCommand> Commands { get; init; }

        public bool Removed { get; set; }
    }
}
