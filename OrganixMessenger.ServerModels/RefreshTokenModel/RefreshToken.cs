using OrganixMessenger.ServerModels.ApplicationUserModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrganixMessenger.ServerModels.RefreshTokenModel
{
    public sealed class RefreshToken
    {
        [Key]
        public int Id { get; init; }

        [Required]
        public Guid UserId { get; init; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; init; }

        [Required]
        public string Token { get; init; }
        [Required]
        public string JWTId { get; init; }

        [Required]
        public DateTime IssuedDate { get; init; }
        [Required]
        public DateTime ExpiryDate { get; init; }
    }
}
