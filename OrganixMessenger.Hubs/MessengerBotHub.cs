namespace OrganixMessenger.Hubs
{
    public class MessengerBotHub(
            IBotRepository botRepository,
            IHubContext<MessengerHub, IMessengerHub> messengerHub
        ) : Hub<IMessengerHub>
    {
        public override async Task OnConnectedAsync()
        {
            await UpdateOnlineStatus();
        }

        public async Task UpdateTypingStatus(TypingType typingType)
        {
            string userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = (await botRepository.GetAsync(Guid.Parse(userId)))!;

            await UpdateOnlineStatus();

            await Clients.Others.ReceiveTypingStatuses(user.ToMessageSenderDTO(), typingType);
            await messengerHub.Clients.All.ReceiveTypingStatuses(user.ToMessageSenderDTO(), typingType);
        }

        public async Task UpdateOnlineStatus()
        {
            string userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = (await botRepository.GetAsync(Guid.Parse(userId)))!;

            user.LastOnline = DateTime.UtcNow;

            await botRepository.SaveAsync();

            await Clients.All.ReceiveProfileUpdates(user.ToMessageSenderDTO());
            await messengerHub.Clients.All.ReceiveProfileUpdates(user.ToMessageSenderDTO());
        }
    }
}
