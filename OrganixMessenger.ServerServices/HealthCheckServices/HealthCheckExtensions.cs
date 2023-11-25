namespace OrganixMessenger.ServerServices.HealthCheckServices
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddConfiguredHealthChecks(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetService<IConfiguration>()!;

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddHealthChecks()
                .AddNpgSql(connectionString);

            services.AddHealthChecksUI(setup =>
            {
                setup.AddHealthCheckEndpoint("Organix", "api/healthz");
            }).AddInMemoryStorage();

            return services;
        }

        public static IApplicationBuilder UseConfiguredHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecksUI(setup =>
            {
                setup.PageTitle = "Жив ли Organix?";
                setup.UIPath = "/health";
                setup.ApiPath = "/api/health";
                setup.WebhookPath = "/api/webhook";
                setup.AsideMenuOpened = false;
            }).UseHealthChecks("/api/healthz", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
                },
            });

            return app;
        }
    }
}