namespace OrganixMessenger.Shared.API.DTOs
{
    public sealed class BotDTO
    {
        public Guid Id { get; init; }

        public string Name { get; init; }

        public UserDTO Owner { get; init; }

        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<BotCommandDTO> Commands { get; init; }
    }
}
