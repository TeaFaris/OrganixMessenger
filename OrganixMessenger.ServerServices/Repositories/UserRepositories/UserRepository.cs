using OrganixMessenger.ServerData;

namespace OrganixMessenger.ServerServices.Repositories.UserRepositories
{
    public sealed class UserRepository(ApplicationDBContext applicationDbContext) : IUserRepository
    {
        public async Task AddAsync(ApplicationUser entity)
        {
            await applicationDbContext.Users.AddAsync(entity);
        }

        public Task AddRangeAsync(IEnumerable<ApplicationUser> entities)
        {
            return applicationDbContext.Users.AddRangeAsync(entities);
        }

        public async Task<IEnumerable<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            return await applicationDbContext
                .Users
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await applicationDbContext
                .Users
                .ToListAsync();
        }

        public Task<ApplicationUser?> GetAsync(Guid id)
        {
            return applicationDbContext
                .Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task RemoveAsync(ApplicationUser entity)
        {
            applicationDbContext.Users.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync(IEnumerable<ApplicationUser> entities)
        {
            applicationDbContext.Users.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public Task SaveAsync()
        {
            return applicationDbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(ApplicationUser entity)
        {
            applicationDbContext.Users.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<ApplicationUser> entities)
        {
            applicationDbContext.Users.UpdateRange(entities);
            return Task.CompletedTask;
        }
    }
}
