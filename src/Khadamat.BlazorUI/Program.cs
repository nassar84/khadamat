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

// Configure base address for HTTP client
var apiBaseUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5144";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

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
builder.Services.AddScoped<WebShareService>();

await builder.Build().RunAsync();
