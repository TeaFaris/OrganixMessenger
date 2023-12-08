namespace OrganixMessenger.ServerServices.Repositories.BotRepositories
{
    public sealed class BotRepository(ApplicationDBContext dbContext) : IBotRepository
    {
        public async Task AddAsync(ApplicationBot entity)
        {
            await dbContext.Bots.AddAsync(entity);
        }

        public Task AddRangeAsync(IEnumerable<ApplicationBot> entities)
        {
            return dbContext.Bots.AddRangeAsync(entities);
        }

        public async Task<IEnumerable<ApplicationBot>> FindAsync(Expression<Func<ApplicationBot, bool>> predicate)
        {
            return await dbContext
                .Bots
                .Include(x => x.ProfilePicture)
                .Include(x => x.Owner)
                .Include(x => x.Commands)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationBot>> GetAllAsync()
        {
            return
                await dbContext
                .Bots
                .Include(x => x.ProfilePicture)
                .Include(x => x.Owner)
                .Include(x => x.Commands)
                .ToListAsync();
        }

        public async Task<ApplicationBot?> GetAsync(Guid id)
        {
            return await dbContext
                .Bots
                .Include(x => x.ProfilePicture)
                .Include(x => x.Owner)
                .Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task RemoveAsync(ApplicationBot entity)
        {
            dbContext.Bots.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync(IEnumerable<ApplicationBot> entities)
        {
            dbContext.Bots.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public Task SaveAsync()
        {
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(ApplicationBot entity)
        {
            dbContext.Bots.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<ApplicationBot> entities)
        {
            dbContext.Bots.UpdateRange(entities);
            return Task.CompletedTask;
        }
    }
}
