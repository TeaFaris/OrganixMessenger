using OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel;

namespace OrganixMessenger.ServerModels.BotCommandModel
{
    public sealed class BotCommand
    {
        [Key]
        public int Id { get; init; }

        [Required]
        public string Trigger { get; set; }

        [Required]
        [StringLength(24, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Description { get; set; }

        public Guid BotId { get; init; }

        [ForeignKey(nameof(BotId))]
        public ApplicationBot Bot { get; init; }
    }
}
