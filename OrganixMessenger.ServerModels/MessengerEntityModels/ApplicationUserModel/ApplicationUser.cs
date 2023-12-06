using OrganixMessenger.ServerModels.FileModel;

namespace OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel
{
    public sealed class ApplicationUser : MessengerEntity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        [Required]
        public string VereficationToken { get; init; }

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpires { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public Role Role { get; set; }
    }
}