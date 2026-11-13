using Infin8.Coapp.UI.Client.Providers;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();

// TokenRefreshHandler renews the 15-minute auth-token cookie on a 401 and retries the call once.
builder.Services.AddScoped(sp => new HttpClient(new TokenRefreshHandler { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<TransactionStateService>();
builder.Services.AddScoped<AppState>();

await builder.Build().RunAsync();
