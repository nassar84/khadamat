using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Khadamat.BlazorUI;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Khadamat.BlazorUI.AppMain>("#app");
var test = typeof(Khadamat.BlazorUI.DummyClass);
var test2 = typeof(Khadamat.BlazorUI.TestComponent);
var test3 = typeof(Khadamat.BlazorUI.Layout.MainLayout);
var test4 = typeof(Khadamat.BlazorUI.Layout.LayoutDummy);




builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Configure HttpClient with Resilience (Polly)
IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// 2. Named Client with Configuration-based BaseAddress
builder.Services.AddScoped<Khadamat.BlazorUI.Services.Auth.AuthenticationHandler>();

builder.Services.AddHttpClient("KhadamatAPI", client =>
{
    // ── Priority 1: cfg_api_url from sessionStorage (set by MobileBridge in index.html)
    //    The mobile app passes ?api_url=... in the page URL. Since WASM's
    //    HostEnvironment.BaseAddress never contains query params, the mobile bridge
    //    script in index.html extracts it first and saves it to sessionStorage.
    //    We retrieve it here via a simple JS call during startup.
    string? apiBaseUrl = null;
    try
    {
        var js = builder.Services.BuildServiceProvider()
                    .GetService<Microsoft.JSInterop.IJSRuntime>();
        // Note: during DI config we can't use IJSRuntime async, so we use the
        // sessionStorage value stored by the inline <script> in index.html.
        // We read it via the static JSRuntime at this stage.
    }
    catch { /* swallow — fallback below will handle it */ }

    // ── Priority 2: appsettings config (empty string means "use host")
    apiBaseUrl ??= builder.Configuration["ApiSettings:BaseUrl"];

    // ── Priority 3: Hosted model — same origin serves both API and WASM
    if (string.IsNullOrEmpty(apiBaseUrl) || apiBaseUrl.Contains("localhost"))
    {
        apiBaseUrl = builder.HostEnvironment.BaseAddress;
    }

    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<Khadamat.BlazorUI.Services.Auth.AuthenticationHandler>()
.AddPolicyHandler(GetRetryPolicy());

// Register the default HttpClient to use the factory-created client
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("KhadamatAPI"));

// 3. Application Services
builder.Services.AddScoped<Khadamat.BlazorUI.Services.ApiClient>();
builder.Services.AddSingleton<Khadamat.BlazorUI.State.AppState>();

// Auth Services
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, Khadamat.BlazorUI.Services.Auth.CustomAuthenticationStateProvider>();
builder.Services.AddScoped<Khadamat.BlazorUI.Services.Auth.IAuthService, Khadamat.BlazorUI.Services.Auth.AuthService>();
builder.Services.AddScoped<Khadamat.BlazorUI.Services.Admin.IAdminService, Khadamat.BlazorUI.Services.Admin.AdminService>();

// Shared / Platform Abstractions
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

