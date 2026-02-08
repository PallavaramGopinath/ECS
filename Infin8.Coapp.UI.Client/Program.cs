using Infin8.Coapp.UI.Client.Providers;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();

builder.Services.AddScoped(sp =>
{
    var baseUri = new Uri(builder.HostEnvironment.IsDevelopment()
    ? "https://localhost:7073/"
    : "https://yourdomain.com/");

    //var handler = new HttpClientHandler
    //{
    //    UseCookies = true,
    //};

    return new HttpClient()
    {
        BaseAddress = baseUri
    };
    //var baseUri = new Uri(builder.HostEnvironment.IsDevelopment()
    //? "https://localhost:7073/"
    //: "https://yourdomain.com/");

    //var h2 = new weba
    //{
    //    // include cookies on requests (needed to send auth-token cookie)
    //    FetchOptions = new FetchOptions
    //    {
    //        Credentials = FetchCredentialsOption.Include
    //    }
    //};

    //var handler = new BrowserHttpMessageHandler
    //{
    //    FetchOptions = new FetchOptions
    //    {
    //        // include cookies on requests (needed to send auth-token cookie)
    //        Credentials = FetchCredentialsOption.Include
    //    }
    //};

    //return new HttpClient(h2)
    //{
    //    BaseAddress = baseUri
    //};
});

builder.Services.AddScoped<TransactionStateService>();
builder.Services.AddScoped<AppState>();

await builder.Build().RunAsync();
