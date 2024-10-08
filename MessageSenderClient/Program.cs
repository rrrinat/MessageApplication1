using MessageSenderClient;
using MessageSenderClient.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var apiUrl = "http://localhost:5009/api/messages";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });


builder.Services.AddScoped<IMessageService, DefaultMessageService>();

await builder.Build().RunAsync();
