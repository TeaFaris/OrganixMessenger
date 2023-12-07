namespace OrganixMessenger.ServerModels.MessageModel
{
    public static class MessageMapper
    {
        public static MessageDTO ToDTO(this Message message)
        {
            return new MessageDTO
            {
                Id = message.Id,
                CustomUsername = message.CustomUsername,
                CustomProfilePicture = message.CustomProfilePicture?.ToDTO(),
                Files = message.Files.ConvertAll(x => x.ToDTO()),
                SendTime = message.SendTime,
                Sender = message.Sender.ToMessageSenderDTO(),
                Edited = message.Edited,
                MessageReplyId = message.MessageReplyId,
                Text = message.Text
            };
        }
    }
}
