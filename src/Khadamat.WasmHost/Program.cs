using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Khadamat.BlazorUI;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5144/") });

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
builder.Services.AddScoped<Khadamat.Shared.Interfaces.ILocationService, Khadamat.BlazorUI.Services.WebLocationService>();

await builder.Build().RunAsync();
