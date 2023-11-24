using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using OrganixMessenger.Base;
using OrganixMessenger.Controllers.Util;
using OrganixMessenger.ServerConfigurations;
using OrganixMessenger.ServerData;
using OrganixMessenger.ServerServices.EmailServices;
using OrganixMessenger.ServerServices.HttpContextServices;
using OrganixMessenger.ServerServices.JWTTokenGeneratorService;
using OrganixMessenger.ServerServices.Repositories.RefreshTokenRepositories;
using OrganixMessenger.ServerServices.Repositories.UserRepositories;
using OrganixMessenger.ServerServices.UserAuthenticationManagerService;
using Serilog;
using Serilog.Ui.PostgreSqlProvider;
using Serilog.Ui.Web;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Configs
builder.Services
    .Configure<JWTSettings>(config.GetSection(nameof(JWTSettings)));
builder.Services
    .Configure<EmailServiceSettings>(config.GetSection(nameof(EmailServiceSettings)));

// Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ASP.NET services
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Responses).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Logging
var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Host.UseSerilog((context, options) =>
{
    options.WriteTo.PostgreSQL(connectionString, "logs", needAutoCreateTable: true)
        .MinimumLevel.Information();

    if (context.HostingEnvironment.IsDevelopment())
    {
        options.WriteTo.Console()
            .MinimumLevel.Information();
    }
});

builder.Services.AddSerilogUi(options =>
{
    options.UseNpgSql(connectionString, "logs");
});

// JWT Authentication
var key = Encoding.UTF8.GetBytes(config["JWTSettings:Key"]!);

var tokenValidationParameter = new TokenValidationParameters
{
    ValidIssuer = config["JWTSettings:Issuer"],
    ValidAudience = config["JWTSettings:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true
};

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameter;
    });

builder.Services.AddAuthorization();

// Database
builder.Services.AddApplicationDBContext();

// Custom services
//// Repositories
builder.Services
    .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services
    .AddScoped<IUserRepository, UserRepository>();

//// Authentication
builder.Services
    .AddScoped<IJWTTokenGenerator, JWTTokenGenerator>();

builder.Services
    .AddScoped<IUserAuthenticationManager, UserAuthenticationManager>();

//// Other
builder.Services
    .AddSingleton<IEmailSender, SmtpEmailSender>();

builder.Services
    .AddSingleton<IHttpContextService, HttpContextService>();

// API Versioning
builder.Services
       .AddApiVersioning(options =>
       {
           options.AssumeDefaultVersionWhenUnspecified = true;
           options.DefaultApiVersion = new ApiVersion(1, 0);
           options.ReportApiVersions = true;
       })
       .AddApiExplorer(options =>
       {
           options.SubstituteApiVersionInUrl = true;
       });

// API Documentation
builder.Services
    .AddOpenApiDocument(options =>
    {
        options.DocumentName = "v1.0";

        options.PostProcess = document =>
        {
            document.Info = new OpenApiInfo
            {
                Version = "v1.0",
                Title = "Organix Messenger API",
                Description = "API documentation for client-side calls or bot creation."
            };
        };

        options.OperationProcessors.Add(new OperationSecurityScopeProcessor("Auth Token"));

        options.AddSecurity("Auth Token", new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "Authorization",
            Description = "Add 'Bearer ' + valid JWT token for client-authorization or add 'Bot ' + valid Bot API token for bot-authorization.",
            In = OpenApiSecurityApiKeyLocation.Header
        });

        options.ApiGroupNames = [ "1.0" ];
    });

var app = builder.Build();

app.UseApplicationDBContext();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// ASP.NET Pipelines
app.UseRouting();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OrganixMessenger.Client._Imports).Assembly);

// Logging
app.UseSerilogRequestLogging();

app.UseSerilogUi(options =>
{
    options.HomeUrl = "";
    options.RoutePrefix = "admin/logs";
});

// API Documentation
app.UseOpenApi(options =>
{
    options.Path = "/api/documentation/{documentName}/documentation.json";
});

app.UseSwaggerUi(options =>
{
    options.DocumentPath = "/api/documentation/{documentName}/documentation.json";

    options.DocumentTitle = "Swagger API Documentation";
    options.Path = "/api/documentation/swagger";
});

app.UseReDoc(options =>
{
    options.DocumentPath = "/api/documentation/{documentName}/documentation.json";
    options.Path = "/api/documentation/redoc";
});


app.Run();
