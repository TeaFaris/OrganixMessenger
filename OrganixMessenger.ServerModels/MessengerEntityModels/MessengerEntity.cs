namespace OrganixMessenger.ServerModels.MessengerEntityModels
{
    public class MessengerEntity
    {
        [Key]
        public Guid Id { get; init; }

        [Required]
        public string Name { get; set; }

        public Guid? ProfilePictureId { get; set; }
        [ForeignKey(nameof(ProfilePictureId))]
        public UploadedFile? ProfilePicture { get; set; }

        [Required]
        public DateTime LastOnline { get; set; }
    }
}
