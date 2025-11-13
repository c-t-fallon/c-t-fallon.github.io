using BlazorApp;
using BlazorApp.Services.JSInterop;
using BlazorApp.Services.Revit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IJSInteropService, JSInteropService>();
builder.Services.AddScoped<IRevitService, RevitService>();

await builder.Build().RunAsync();
