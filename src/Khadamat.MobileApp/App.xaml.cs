using Khadamat.MobileApp.Services;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace Khadamat.MobileApp;

public partial class App : MauiApp
{
    private readonly Views.WelcomePage _welcomePage;

    public App(Views.WelcomePage welcomePage)
    {
        InitializeComponent();
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
        return new Window(_welcomePage) { Title = "Khadamat" };
    }
}
