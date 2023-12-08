namespace OrganixMessenger.Shared.API.DTOs
{
    public sealed class UserDTO
    {
        public Guid Id { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public FileDTO? ProfilePicture { get; init; }

        public DateTime LastOnline { get; init; }

        public Role Role { get; init; }
    }
}
