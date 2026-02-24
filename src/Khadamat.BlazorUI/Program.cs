using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Khadamat.BlazorUI;
using Khadamat.BlazorUI.Services;
using Khadamat.BlazorUI.State;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure base address for HttpClient
var apiBaseUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5144";

// Register AuthenticationHandler
builder.Services.AddScoped<Khadamat.BlazorUI.Services.Auth.AuthenticationHandler>();

// Register HttpClient with AuthenticationHandler
builder.Services.AddHttpClient("KhadamatAPI", client => 
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<Khadamat.BlazorUI.Services.Auth.AuthenticationHandler>();

// Register the HttpClient as the default scoped service
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("KhadamatAPI"));

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add Authentication
builder.Services.AddScoped<Khadamat.Shared.Interfaces.ISecureStorageService, WebSecureStorageService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, Khadamat.BlazorUI.Services.Auth.CustomAuthenticationStateProvider>();

// Add Application Services
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<SignalRClientService>();
builder.Services.AddScoped<Khadamat.BlazorUI.Services.Auth.IAuthService, Khadamat.BlazorUI.Services.Auth.AuthService>();
builder.Services.AddScoped<Khadamat.BlazorUI.Services.Admin.IAdminService, Khadamat.BlazorUI.Services.Admin.AdminService>();
builder.Services.AddScoped<Khadamat.Application.Interfaces.IOfflineDataService, WebOfflineDataService>();


// Register Web Implementations for Shared Interfaces
builder.Services.AddScoped<WebShareService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.IShareService, WebShareService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.IPhoneService, WebPhoneService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.ILocationService, WebLocationService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.IExternalAuthService, WebExternalAuthService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.INotificationService, WebNotificationService>();
builder.Services.AddScoped<Khadamat.Shared.Interfaces.IBiometricService, WebBiometricService>();

await builder.Build().RunAsync();
