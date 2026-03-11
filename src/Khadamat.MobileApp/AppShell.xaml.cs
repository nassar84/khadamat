using Microsoft.Maui.Controls;

namespace Khadamat.MobileApp
{
    public partial class AppShell : Shell
    {
        private readonly ViewModels.ShellViewModel _viewModel;

        public AppShell(ViewModels.ShellViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            // Load settings in background
            Task.Run(async () => await _viewModel.LoadSettingsAsync());
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
