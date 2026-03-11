using Khadamat.MobileApp.Services;
namespace Khadamat.MobileApp;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        _shell = shell;
        
        DeepLinkBridge.OnDeepLinkReceived += async (url) => 
        {
            if (Shell.Current is AppShell appShell)
            {
                await appShell.HandleDeepLink(url);
            }
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell) { Title = "Khadamat.MobileApp" };
    }
}
