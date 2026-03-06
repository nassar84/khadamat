using Microsoft.Maui.Controls;

namespace Khadamat.MobileApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        public async Task HandleDeepLink(string url)
        {
            // Example: khadamat://service/120 -> /service/120
            var route = url.Replace("khadamat://", "").Replace("https://khadamat.com/", "");
            
            // Navigate to a generic container with this route
            await Shell.Current.GoToAsync($"//HomePage?route={Uri.EscapeDataString(route)}");
        }
    }
}
