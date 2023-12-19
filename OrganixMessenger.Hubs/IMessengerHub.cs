namespace OrganixMessenger.Hubs
{
    public interface IMessengerHub
    {
        public Task ReceiveMessages(MessageDTO message);

        public Task ReceiveTypingStatuses(MessengerSenderDTO typingEntity, TypingType typingType);

        public Task ReceiveProfileUpdates(MessengerSenderDTO typingEntity);

        public Task ReceiveRemovedMessages(int messageId);

        public Task ReceiveEditedMessages(MessageDTO message);
    }
}
