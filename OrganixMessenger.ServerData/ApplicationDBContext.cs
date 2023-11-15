using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OrganixMessenger.ServerModels.ApplicationUserModel;
using OrganixMessenger.ServerModels.RefreshTokenModel;

namespace Organix.ServerData
{
    public sealed class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : DbContext(options)
    {
        public DbSet<ApplicationUser> Users { get; init; }
        public DbSet<RefreshToken> RefreshTokens { get; init; }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
    {
        public ApplicationDBContext CreateDbContext(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Directory.GetCurrentDirectory() + $"/../OrganixMessenger/appsettings.json")
                .AddUserSecrets("40bb3de4-eda5-4d89-b5a3-2c29cd2dbb42")
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDBContext>();
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.UseNpgsql(connectionString);

            return new ApplicationDBContext(builder.Options);
        }
    }
}