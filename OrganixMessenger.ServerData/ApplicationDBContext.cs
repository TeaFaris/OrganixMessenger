using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OrganixMessenger.ServerModels.FileModel;
using OrganixMessenger.ServerModels.MessageModel;
using OrganixMessenger.ServerModels.MessengerEntityModels;
using OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel;
using OrganixMessenger.ServerModels.RefreshTokenModel;

namespace OrganixMessenger.ServerData
{
    public sealed class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : DbContext(options)
    {
        public DbSet<MessengerEntity> MessengerEntities { get; init; }
        public DbSet<ApplicationUser> Users { get; init; }
        public DbSet<RefreshToken> RefreshTokens { get; init; }
        public DbSet<UploadedFile> Files { get; init; }
        public DbSet<Message> Messages { get; init; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessengerEntity>()
                .UseTptMappingStrategy();

            modelBuilder.Entity<Message>()
                .HasMany(x => x.Files)
                .WithOne()
                .HasForeignKey("MessageId");
        }
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