namespace OrganixMessenger.ServerModels.ApplicationUserModel
{
    public sealed class ApplicationUser
    {
        [Key]
        public Guid Id { get; init; }

        [Required]
        public string Username { get; set; }

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