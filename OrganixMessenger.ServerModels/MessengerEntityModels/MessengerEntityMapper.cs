namespace OrganixMessenger.ServerModels.MessengerEntityModels
{
    public static class MessengerEntityMapper
    {
        public static MessengerSenderDTO ToMessageSenderDTO(this MessengerEntity entity)
        {
            return new MessengerSenderDTO
            {
                Id = entity.Id,
                IsBot = entity is not ApplicationUser
            };
        }
    }
}
