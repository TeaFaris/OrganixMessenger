using OrganixMessenger.Base;
using OrganixMessenger.Configuration;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Configs
builder.Services
    .Configure<JWTSettings>(config.GetSection(nameof(JWTSettings)));

// Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ASP.NET services
builder.Services.AddControllers();

var app = builder.Build();

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
app.UseAntiforgery();

// ASP.NET Pipelines
app.UseRouting();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OrganixMessenger.Client._Imports).Assembly);

app.Run();
