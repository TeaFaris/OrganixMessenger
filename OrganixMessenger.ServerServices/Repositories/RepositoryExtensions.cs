namespace OrganixMessenger.ServerServices.Repositories
{
    public static class RepositoryExtensions
    {
        public static async Task<TEntity?> FindFirstOrDefaultAsync<TEntity, TID>(this IRepository<TEntity, TID> repository, Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            var entities = await repository.FindAsync(predicate);
            return entities.FirstOrDefault();
        }
    }
}
