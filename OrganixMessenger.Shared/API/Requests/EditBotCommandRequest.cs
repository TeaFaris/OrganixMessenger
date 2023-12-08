namespace OrganixMessenger.Shared.API.Requests
{
    public sealed class EditBotCommandRequest
    {
        public string? Trigger { get; init; }

        public string? Name { get; init; }

        public string? Description { get; init; }
    }
}
