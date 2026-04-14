using Khadamat.MobileApp.Services;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace Khadamat.MobileApp;

public partial class App : MauiApp
{
    private readonly Views.WelcomePage _welcomePage;

    public App(Views.WelcomePage welcomePage)
    {
        try 
        {
            InitializeComponent();
            _welcomePage = welcomePage;

            // Simple safe theme initialization
            string savedTheme = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", "default");
            ApplyInitialTheme(savedTheme);

            // Handle DeepLink
            DeepLinkBridge.OnDeepLinkReceived += async (url) => 
            {
                if (Microsoft.Maui.Controls.Application.Current != null && Shell.Current != null)
                {
                    await Shell.Current.GoToAsync(url);
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: [CRITICAL APP INIT ERROR] {ex.Message}");
        }
    }

    private void ApplyInitialTheme(string theme)
    {
        try 
        {
            string primaryHex = theme switch
            {
                "sunset" => "#ea580c",
                "ocean" => "#0284c7",
                "forest" => "#16a34a",
                "lavender" => "#9333ea",
                "royal" => "#db2777",
                _ => "#6366f1"
            };
            Resources["Primary"] = Color.FromArgb(primaryHex);
        }
        catch { }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Page initialPage;
        bool hasCompleted = Microsoft.Maui.Storage.Preferences.Default.Get("HasCompletedOnboarding", false);
        
        if (hasCompleted)
        {
            // By-pass the Welcome page if they have already opened the app before
            initialPage = _welcomePage.AppShellInstance;
        }
        else
        {
            initialPage = _welcomePage;
        }

        var window = new Window(initialPage) { Title = "Khadamat" };
        window.FlowDirection = FlowDirection.RightToLeft;
        return window;
    }
}
