using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped(http => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7073/"),
});
builder.Services.AddSingleton<TransactionStateService>();
await builder.Build().RunAsync();
