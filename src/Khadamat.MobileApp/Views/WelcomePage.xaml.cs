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
        private readonly IAudioService _audioService;
        private readonly IAudioManager _audioManager;
        private readonly IExternalAuthService _externalAuth;
        private readonly IConfiguration _configuration;

        public WelcomePage(AppShell shell, IAudioManager audioManager, IExternalAuthService externalAuth, IConfiguration configuration, IAudioService audioService)
        {
            InitializeComponent();
            _shell = shell;
            _audioManager = audioManager;
            _externalAuth = externalAuth;
            _configuration = configuration;
            _audioService = audioService;
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
                        {
                            Preferences.Default.Set("HasCompletedOnboarding", true);
                            MauiApp.Current.MainPage = _shell;
                        }
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
            
            // Check connectivity and reachability of the API on startup
            await CheckApiConnection();
            
            // Load Settings and play startup sound
            if (_shell.BindingContext is ViewModels.ShellViewModel vm)
            {
                await vm.LoadSettingsAsync();
                string soundFile = string.IsNullOrEmpty(vm.OpenAppSound) ? "bic_ring1.mp3" : vm.OpenAppSound;
                await _audioService.PlaySoundAsync(soundFile);
            }
        }

        private async Task CheckApiConnection()
        {
            string apiBaseUrl = Preferences.Default.Get("ApiBaseUrl", _configuration["ApiSettings:BaseUrl"] ?? "https://jobsek.eis-dev.com");
            try
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: Testing connectivity to: {apiBaseUrl}");
                
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync($"{apiBaseUrl.TrimEnd('/')}/v1/settings");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("ANTIGRAVITY_LOG: API is REACHABLE! (Success)");
                }
                else
                {
                    Console.WriteLine($"ANTIGRAVITY_LOG: [WARNING] API response was {response.StatusCode} for URL {apiBaseUrl}");
                    // Do not show error to user immediately, maybe it's fine for some cases, 
                    // but log it extensively.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: [ERROR] API UNREACHABLE: {ex.Message}");
                // This is critical. Might be wrong URL or no internet.
                await DisplayAlert("تنبيه الاتصال", 
                    $"لا يمكن الوصول إلى السيرفر حالياً على العنوان:\n{apiBaseUrl}\n\nيرجى التأكد من اتصال الإنترنت أو إعدادات السيرفر.", 
                    "موافق");
            }
        }

        public AppShell GetShell() => _shell;
        public AppShell AppShellInstance => _shell;

        private async void OnDevSettingsTriggered(object sender, EventArgs e)
        {
            string currentWebUrl = Preferences.Default.Get("WebAppBaseUrl", "http://10.0.2.2:5144");
            string currentApiUrl = Preferences.Default.Get("ApiBaseUrl", "http://10.0.2.2:5144");

            string webUrl = await DisplayPromptAsync("إعدادات المطور (Web)", 
                "أدخل عنوان الـ Web App:", 
                "التالي", "إلغاء", 
                "http://", 200, Keyboard.Url, currentWebUrl);

            if (string.IsNullOrWhiteSpace(webUrl)) return;

            string apiUrl = await DisplayPromptAsync("إعدادات المطور (API)", 
                "أدخل عنوان الـ API:", 
                "حفظ", "إلغاء", 
                "http://", 200, Keyboard.Url, currentApiUrl);

            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                if (!webUrl.StartsWith("http")) webUrl = "http://" + webUrl;
                if (!apiUrl.StartsWith("http")) apiUrl = "http://" + apiUrl;

                Preferences.Default.Set("WebAppBaseUrl", webUrl);
                Preferences.Default.Set("ApiBaseUrl", apiUrl);

                await DisplayAlert("نجاح", "تم حفظ الإعدادات. سيتم استخدام العناوين الجديدة عند إعادة تحميل الصفحة.", "تم");
            }
        }

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
                
                Preferences.Default.Set("HasCompletedOnboarding", true);
                MauiApp.Current.MainPage = _shell;
                
                // Explicitly navigate to Home page so it always opens first
                await Shell.Current.GoToAsync("//HomePage");
            }
        }

        private async void OnStandardLoginClicked(object sender, EventArgs e)
        {
            if (MauiApp.Current != null)
            {
                await ((VisualElement)sender).ScaleTo(0.9, 100);
                await ((VisualElement)sender).ScaleTo(1.0, 100);
                
                Preferences.Default.Set("HasCompletedOnboarding", true);
                MauiApp.Current.MainPage = _shell;
                
                // Navigate to login page
                await Shell.Current.GoToAsync("//HomePage?route=login");
            }
        }
    }
}
