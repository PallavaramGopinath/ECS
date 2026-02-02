using Infin8.Coapp.UI.Client.Providers;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();

builder.Services.AddScoped(sp =>
{
    var handler = new HttpClientHandler();
    return new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.IsDevelopment()
            ? "https://localhost:7073/"
            : "https://yourdomain.com/")
    };
});

builder.Services.AddScoped<TransactionStateService>();
builder.Services.AddScoped<AppState>();

await builder.Build().RunAsync();
