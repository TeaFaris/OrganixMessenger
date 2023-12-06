namespace OrganixMessenger.ServerServices.Repositories.MessageRepositories
{
    public interface IMessageRepository : IRepository<Message, int>
    {
        public Task<IEnumerable<Message>> GetSliceOfMessagesAsync(int from, int count, Expression<Func<Message, bool>>? predicate = null);
    }
}
