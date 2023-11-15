using System.ComponentModel.DataAnnotations;

namespace OrganixMessenger.Configuration
{
    public sealed class JWTSettings
    {
        [Required]
        public string Key { get; init; }

        [Required]
        [Url]
        public string Audience { get; init; }

        [Required]
        [Url]
        public string Issuer { get; init; }

        [Required]
        public TimeSpan ExpiryTime { get; init; }
    }
}
