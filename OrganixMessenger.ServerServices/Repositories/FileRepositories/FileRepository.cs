namespace OrganixMessenger.ServerServices.Repositories.FileRepositories
{
    public sealed class FileRepository(ApplicationDBContext dbContext) : IFileRepository
    {
        public async Task AddAsync(UploadedFile entity)
        {
            await dbContext.Files.AddAsync(entity);
        }

        public Task AddRangeAsync(IEnumerable<UploadedFile> entities)
        {
            return dbContext.Files.AddRangeAsync(entities);
        }

        public async Task<IEnumerable<UploadedFile>> FindAsync(Expression<Func<UploadedFile, bool>> predicate)
        {
            return await dbContext
                .Files
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<UploadedFile>> GetAllAsync()
        {
            return await dbContext
                .Files
                .ToListAsync();
        }

        public Task<UploadedFile?> GetAsync(Guid id)
        {
            return dbContext
                .Files
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task RemoveAsync(UploadedFile entity)
        {
            dbContext.Files.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync(IEnumerable<UploadedFile> entities)
        {
            dbContext.Files.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public Task SaveAsync()
        {
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(UploadedFile entity)
        {
            dbContext.Files.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<UploadedFile> entities)
        {
            dbContext.Files.UpdateRange(entities);
            return Task.CompletedTask;
        }
    }
}
