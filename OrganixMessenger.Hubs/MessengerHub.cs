namespace OrganixMessenger.Hubs
{
    [Authorize]
    public class MessengerHub(
            IUserRepository userRepository,
            IHubContext<MessengerBotHub, IMessengerHub> messengerBotHub
        ) : Hub<IMessengerHub>
    {
        public override async Task OnConnectedAsync()
        {
            await UpdateOnlineStatus();
        }

        public async Task UpdateTypingStatus(TypingType typingType)
        {
            string userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = (await userRepository.GetAsync(Guid.Parse(userId)))!;

            await UpdateOnlineStatus();

            await Clients.All.ReceiveTypingStatuses(user.ToMessageSenderDTO(), typingType);
            await messengerBotHub.Clients.All.ReceiveTypingStatuses(user.ToMessageSenderDTO(), typingType);
        }

        public async Task UpdateOnlineStatus()
        {
            string userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = (await userRepository.GetAsync(Guid.Parse(userId)))!;

            user.LastOnline = DateTime.UtcNow;

            await userRepository.SaveAsync();

            await Clients.All.ReceiveProfileUpdates(user.ToMessageSenderDTO());
            await messengerBotHub.Clients.All.ReceiveProfileUpdates(user.ToMessageSenderDTO());
        }
    }
}
