namespace OrganixMessenger.ServerServices.Repositories.MessageRepositories
{
	public class MessageRepository(ApplicationDBContext dbContext) : IMessageRepository
	{
		public async Task AddAsync(Message entity)
		{
			await dbContext.Messages.AddAsync(entity);
		}

		public Task AddRangeAsync(IEnumerable<Message> entities)
		{
			return dbContext.Messages.AddRangeAsync(entities);
		}

		public async Task<IEnumerable<Message>> FindAsync(Expression<Func<Message, bool>> predicate)
		{
			return await dbContext
				.Messages
				.Include(x => x.Sender)
				.Include(x => x.MessageReply)
				.Include(x => x.Files)
				.Where(predicate)
				.ToListAsync();
		}

		public async Task<IEnumerable<Message>> GetAllAsync()
		{
			return
				await dbContext
				.Messages
				.Include(x => x.Sender)
				.Include(x => x.MessageReply)
				.Include(x => x.Files)
				.ToListAsync();
		}

		public async Task<Message?> GetAsync(int id)
		{
			return await dbContext
				.Messages
				.Include(x => x.Sender)
				.Include(x => x.MessageReply)
				.Include(x => x.Files)
				.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task<IEnumerable<Message>> GetSliceOfMessagesAsync(int from, int count, Expression<Func<Message, bool>>? predicate = null)
		{
			var lastMessages = dbContext
				.Messages
				.Include(x => x.Sender)
				.Include(x => x.MessageReply)
				.Include(x => x.Files)
				.AsQueryable();

			if (predicate is not null)
			{
				lastMessages = lastMessages.Where(predicate);
			}
			
			lastMessages = lastMessages
				.OrderBy(x => x.SendTime)
				.Skip(from)
				.Take(count);

			return lastMessages;
		}

		public Task RemoveAsync(Message entity)
		{
			dbContext.Messages.Remove(entity);
			return Task.CompletedTask;
		}

		public Task RemoveRangeAsync(IEnumerable<Message> entities)
		{
			dbContext.Messages.RemoveRange(entities);
			return Task.CompletedTask;
		}

		public Task SaveAsync()
		{
			return dbContext.SaveChangesAsync();
		}

		public Task UpdateAsync(Message entity)
		{
			dbContext.Messages.Update(entity);
			return Task.CompletedTask;
		}

		public Task UpdateRangeAsync(IEnumerable<Message> entities)
		{
			dbContext.Messages.UpdateRange(entities);
			return Task.CompletedTask;
		}
	}
}
