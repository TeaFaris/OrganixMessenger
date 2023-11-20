using OrganixMessenger.ServerData;

namespace OrganixMessenger.ServerServices.Repositories.RefreshTokenRepositories
{
    public sealed class RefreshTokenRepository(ApplicationDBContext applicationDbContext) : IRefreshTokenRepository
    {
        public async Task AddAsync(RefreshToken entity)
        {
            await applicationDbContext.RefreshTokens.AddAsync(entity);
        }

        public Task AddRangeAsync(IEnumerable<RefreshToken> entities)
        {
            return applicationDbContext.RefreshTokens.AddRangeAsync(entities);
        }

        public async Task<IEnumerable<RefreshToken>> FindAsync(Expression<Func<RefreshToken, bool>> predicate)
        {
            return await applicationDbContext
                .RefreshTokens
                .Include(x => x.User)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetAllAsync()
        {
            return await applicationDbContext
                .RefreshTokens
                .Include(x => x.User)
                .ToListAsync();
        }

        public Task<RefreshToken?> GetAsync(int id)
        {
            return applicationDbContext
                .RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task RemoveAsync(RefreshToken entity)
        {
            applicationDbContext.RefreshTokens.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync(IEnumerable<RefreshToken> entities)
        {
            applicationDbContext.RefreshTokens.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public Task SaveAsync()
        {
            return applicationDbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(RefreshToken entity)
        {
            applicationDbContext.RefreshTokens.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<RefreshToken> entities)
        {
            applicationDbContext.RefreshTokens.UpdateRange(entities);
            return Task.CompletedTask;
        }
    }
}
