using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Khadamat.BlazorUI;
using Khadamat.BlazorUI.Services.Auth;
using Khadamat.BlazorUI.State;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

using Khadamat.BlazorUI.Services.Admin;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Use the API URL from appsettings or fallback to localhost:5144
var apiUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5144";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// App State
builder.Services.AddScoped<AppState>();

// Services
builder.Services.AddScoped<Khadamat.BlazorUI.Services.ApiClient>();
builder.Services.AddScoped<IAdminService, AdminService>();


await builder.Build().RunAsync();
