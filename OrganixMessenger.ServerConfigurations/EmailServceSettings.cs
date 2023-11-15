using System.ComponentModel.DataAnnotations;

namespace OrganixMessenger.ServerConfigurations
{
    public sealed class EmailServceSettings
    {
        [Required]
        public string Username { get; init; }

        [Required]
        public string Password { get; init; }

        [Required]
        public string Host { get; init; }

        [Required]
        public int Port { get; init; }
    }
}
