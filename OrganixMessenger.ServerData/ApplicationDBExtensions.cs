using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Organix.ServerData;

namespace OrganixMessenger.ServerData
{
    public static class ApplicationDBExtensions
    {
        public static IServiceCollection AddApplicationDBContext(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetService<IConfiguration>()!;

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDBContext>(options
                => options.UseNpgsql(connectionString));

            return services;
        }

        public static IHost UseApplicationDBContext(this IHost app)
        {
            using var provider = app.Services.CreateScope();

            var context = provider.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            return app;
        }
    }
}
