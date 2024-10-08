using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MessageReceiverClient;
using MessageReceiverClient.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var webSocketUrl = "ws://localhost:5009/ws";

builder.Services.AddSingleton<WebSocketService>(sp =>
    new WebSocketService(sp.GetRequiredService<NavigationManager>(), sp.GetRequiredService<ILogger<WebSocketService>>(), webSocketUrl));

await builder.Build().RunAsync();
