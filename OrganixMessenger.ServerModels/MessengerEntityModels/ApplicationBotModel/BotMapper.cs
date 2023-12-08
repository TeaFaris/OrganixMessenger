namespace OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationBotModel
{
    public static class BotMapper
    {
        public static BotDTO ToDTO(this ApplicationBot bot)
        {
            return new BotDTO
            {
                Id = bot.Id,
                Name = bot.Name,
                Owner = bot.Owner.ToDTO(),
                Commands = bot.Commands.Select(x => x.ToDTO(excludeBot: true)).ToList()
            };
        }
    }
}
