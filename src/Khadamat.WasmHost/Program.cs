using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Khadamat.BlazorUI;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<Khadamat.BlazorUI.Services.Auth.AuthenticationHandler>();

builder.Services.AddHttpClient("KhadamatAPI", (sp, client) => 
{
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    var uri = new Uri(nav.BaseUri);
    // If we're on a local address, use the same host but the API port
    if (uri.Host == "localhost" || uri.Host == "127.0.0.1" || uri.Host == "10.0.2.2")
    {
        client.BaseAddress = new Uri($"{uri.Scheme}://{uri.Host}:5144/");
    }
    else
    {
        // Production fallback (change this to your production API URL)
        client.BaseAddress = new Uri("http://localhost:5144/");
    }
})
.AddHttpMessageHandler<Khadamat.BlazorUI.Services.Auth.AuthenticationHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("KhadamatAPI"));

builder.Services.AddScoped<Khadamat.BlazorUI.Services.ApiClient>();
builder.Services.AddSingleton<Khadamat.BlazorUI.State.AppState>();

// Auth Services
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, Khadamat.BlazorUI.Services.Auth.CustomAuthenticationStateProvider>();
builder.Services.AddScoped<Khadamat.BlazorUI.Services.Auth.IAuthService, Khadamat.BlazorUI.Services.Auth.AuthService>();
builder.Services.AddScoped<Khadamat.BlazorUI.Services.Admin.IAdminService, Khadamat.BlazorUI.Services.Admin.AdminService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.IShareService, Khadamat.BlazorUI.Services.WebShareService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.INotificationService, Khadamat.BlazorUI.Services.WebNotificationService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.IPhoneService, Khadamat.BlazorUI.Services.WebPhoneService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.ISecureStorageService, Khadamat.BlazorUI.Services.WebSecureStorageService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.IExternalAuthService, Khadamat.BlazorUI.Services.WebExternalAuthService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.IBiometricService, Khadamat.BlazorUI.Services.WebBiometricService>();
builder.Services.AddScoped<Khadamat.Application.Interfaces.IOfflineDataService, Khadamat.BlazorUI.Services.WebOfflineDataService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.ILocationService, Khadamat.BlazorUI.Services.WebLocationService>();
builder.Services.AddScoped<Khadamat.BlazorUI.Services.SignalRClientService>();
builder.Services.AddScoped<Khadamat.BlazorUI.Services.SoundService>();

await builder.Build().RunAsync();
