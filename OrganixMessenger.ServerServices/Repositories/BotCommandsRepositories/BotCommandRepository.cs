namespace OrganixMessenger.ServerServices.Repositories.BotCommandsRepositories
{
    public sealed class BotCommandRepository(ApplicationDBContext dbContext) : IBotCommandRepository
    {
        public async Task AddAsync(BotCommand entity)
        {
            await dbContext.Commands.AddAsync(entity);
        }

        public Task AddRangeAsync(IEnumerable<BotCommand> entities)
        {
            return dbContext.Commands.AddRangeAsync(entities);
        }

        public async Task<IEnumerable<BotCommand>> FindAsync(Expression<Func<BotCommand, bool>> predicate)
        {
            return await dbContext
                .Commands
                .Include(x => x.Bot)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BotCommand>> GetAllAsync()
        {
            return
                await dbContext
                .Commands
                .Include(x => x.Bot)
                .ToListAsync();
        }

        public async Task<BotCommand?> GetAsync(int id)
        {
            return await dbContext
                .Commands
                .Include(x => x.Bot)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task RemoveAsync(BotCommand entity)
        {
            dbContext.Commands.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync(IEnumerable<BotCommand> entities)
        {
            dbContext.Commands.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public Task SaveAsync()
        {
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(BotCommand entity)
        {
            dbContext.Commands.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<BotCommand> entities)
        {
            dbContext.Commands.UpdateRange(entities);
            return Task.CompletedTask;
        }
    }
}
