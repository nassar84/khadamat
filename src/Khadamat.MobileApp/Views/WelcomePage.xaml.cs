using System;
using MauiApp = Microsoft.Maui.Controls.Application;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using Khadamat.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Khadamat.MobileApp.Views
{
    public partial class WelcomePage : ContentPage
    {
        private readonly AppShell _shell;
        private readonly IAudioManager _audioManager;
        private readonly IExternalAuthService _externalAuth;
        private readonly IConfiguration _configuration;

        public WelcomePage(AppShell shell, IAudioManager audioManager, IExternalAuthService externalAuth, IConfiguration configuration)
        {
            InitializeComponent();
            _shell = shell;
            _audioManager = audioManager;
            _externalAuth = externalAuth;
            _configuration = configuration;
            BindingContext = _shell.BindingContext;
        }

        public async void OnGoogleLoginClicked(object sender, EventArgs e) => await SocialLogin("Google");
        public async void OnFacebookLoginClicked(object sender, EventArgs e) => await SocialLogin("Facebook");

        private async Task SocialLogin(string provider)
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/');
                if (string.IsNullOrEmpty(apiBaseUrl)) return;

                var callbackUrl = "khadamat://callback";
                var authUrl = $"{apiBaseUrl}/v1/auth/external-login?provider={provider}&redirectUrl={Uri.EscapeDataString(callbackUrl)}";

                var result = await _externalAuth.AuthenticateAsync(provider, authUrl, "khadamat");
                
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    // Success! Update Shell and transition
                    if (_shell.BindingContext is ViewModels.ShellViewModel vm)
                    {
                        // Save to secure storage so Blazor part can find it later
                        await SecureStorage.SetAsync("authToken", result.Token);
                        if (!string.IsNullOrEmpty(result.RefreshToken))
                            await SecureStorage.SetAsync("refreshToken", result.RefreshToken);

                        // Trigger state update in UI
                        vm.SetAuthenticated(true);
                        
                        if (MauiApp.Current != null)
                            MauiApp.Current.MainPage = _shell;
                    }
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"ANTIGRAVITY_LOG: Social Login Error: {ex.Message}");
                 await DisplayAlert("خطأ", "فشل تسجيل الدخول الاجتماعي. يرجى المحاولة مرة أخرى.", "تم");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try 
            {
                var player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("startup_sound.mp3"));
                player.Play();
            }
            catch { /* File might be missing */ }
        }

        public AppShell GetShell() => _shell;
        public AppShell AppShellInstance => _shell;

        private async void OnTermsClicked(object sender, EventArgs e)
        {
            // Navigate to terms page but we must ensure we are in the Shell first
            if (MauiApp.Current != null)
            {
                // Play a small animation before switching
                if (sender is VisualElement visual)
                {
                    await visual.ScaleTo(0.9, 100);
                    await visual.ScaleTo(1.0, 100);
                }
                
                // Set the main page to the shell
                MauiApp.Current.MainPage = _shell;
                
                // Then navigate to the correct terms route (/terms in Blazor)
                await Shell.Current.GoToAsync("//HomePage?route=terms");
            }
        }

        private async void OnStartClicked(object sender, EventArgs e)
        {
            // Transition to the main app shell
            if (MauiApp.Current != null)
            {
                // Play a small animation before switching
                await ((VisualElement)sender).ScaleTo(0.9, 100);
                await ((VisualElement)sender).ScaleTo(1.0, 100);
                
                MauiApp.Current.MainPage = _shell;
            }
        }
    }
}
