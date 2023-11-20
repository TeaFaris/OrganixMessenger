using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
           options.ApiVersionReader = ApiVersionReader.Combine(
                   new HeaderApiVersionReader("X-Version")
               );
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

app.Run();
