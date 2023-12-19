using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<HttpClient>(sp =>
{
    var absoluteUri = sp.GetRequiredService<NavigationManager>().BaseUri;

    return new HttpClient
    {
        BaseAddress = new Uri(absoluteUri)
    };
});

await builder.Build().RunAsync();
