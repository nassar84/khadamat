using Khadamat.MobileApp.Services;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace Khadamat.MobileApp;

public partial class App : MauiApp
{
    private readonly Views.WelcomePage _welcomePage;

    public App(Views.WelcomePage welcomePage)
    {
        InitializeComponent();
        
        // Initial Theme Load
        string savedTheme = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", "default");
        string primaryHex = savedTheme switch
        {
            "sunset" => "#ea580c",
            "ocean" => "#0ea5e9",
            "forest" => "#10b981",
            "lavender" => "#8b5cf6",
            "royal" => "#eab308",
            _ => "#6366f1" // default
        };
        Resources["Primary"] = Color.FromArgb(primaryHex);
        
        _welcomePage = welcomePage;
        
        DeepLinkBridge.OnDeepLinkReceived += async (url) => 
        {
            if (MauiApp.Current != null && MauiApp.Current.MainPage != _welcomePage.AppShellInstance)
            {
                // Note: I need to expose AppShell in WelcomePage or have a way to access it.
                // Actually, since WelcomePage has it injected, I can use it.
            }
            
            // Simplified: If we get a deep link, we probably should ensure we are in the Shell.
            if (MauiApp.Current != null && Shell.Current == null)
            {
                MauiApp.Current.MainPage = _welcomePage.GetShell();
            }

            if (Shell.Current is AppShell appShell)
            {
                await appShell.HandleDeepLink(url);
            }
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Page initialPage;
        bool hasCompleted = Microsoft.Maui.Storage.Preferences.Default.Get("HasCompletedOnboarding", false);
        
        if (hasCompleted)
        {
            // By-pass the Welcome page if they have already opened the app before
            initialPage = _welcomePage.GetShell();
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
