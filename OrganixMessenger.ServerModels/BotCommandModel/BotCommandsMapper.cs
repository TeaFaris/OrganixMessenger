namespace OrganixMessenger.ServerModels.BotCommandModel
{
    public static class BotCommandsMapper
    {
        public static BotCommandDTO ToDTO(this BotCommand command, bool excludeBot = false)
        {
            return new BotCommandDTO
            {
                Id = command.Id,
                Description = command.Description,
                Name = command.Name,
                Trigger = command.Trigger,
                Bot = excludeBot ? null : command.Bot.ToDTO()
            };
        }
    }
}
