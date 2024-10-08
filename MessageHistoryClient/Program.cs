using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MessageHistoryClient;
using MessageHistoryClient.Services;
using Microsoft.Extensions.Logging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var serverUrl = "http://localhost:5009/api/messages";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(serverUrl) });
builder.Services.AddScoped<MessageService>();

await builder.Build().RunAsync();
