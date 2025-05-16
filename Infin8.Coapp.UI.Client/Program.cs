using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped(http => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7073/"),
});

await builder.Build().RunAsync();
