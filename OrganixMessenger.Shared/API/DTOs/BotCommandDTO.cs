namespace OrganixMessenger.Shared.API.DTOs
{
    public sealed class BotCommandDTO
    {
        public int Id { get; init; }

        public string Trigger { get; init; }

        public string Name { get; init; }

        public string Description { get; init; }

        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public BotDTO? Bot { get; init; }
    }
}
