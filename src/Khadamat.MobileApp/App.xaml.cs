using Khadamat.MobileApp.Services;
namespace Khadamat.MobileApp;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		InitializeComponent();
        
        DeepLinkBridge.OnDeepLinkReceived += async (url) => 
        {
            if (Shell.Current is AppShell shell)
            {
                await shell.HandleDeepLink(url);
            }
        };
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell()) { Title = "Khadamat.MobileApp" };
	}
}
