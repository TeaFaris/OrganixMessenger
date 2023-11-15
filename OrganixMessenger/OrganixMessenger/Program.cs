using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrganixMessenger.Base;
using OrganixMessenger.ServerConfigurations;
using OrganixMessenger.ServerData;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Configs
builder.Services
    .Configure<JWTSettings>(config.GetSection(nameof(JWTSettings)));
builder.Services
    .Configure<EmailServceSettings>(config.GetSection(nameof(EmailServceSettings)));

// Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ASP.NET services
builder.Services.AddControllers();

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
