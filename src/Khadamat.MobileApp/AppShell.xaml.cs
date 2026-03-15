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

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            if (args.Target != null)
            {
                var targetLoc = args.Target.Location.ToString();
                var targetPath = targetLoc.Split('?')[0];
                
                // If navigating to the Home page (either by tab click or menu)
                // and it's either the exact same location or a base navigation without params,
                // we force the WebView to return to root.
                if (targetPath.EndsWith("HomePage") || targetPath == "//")
                {
                    // If no specific route param is provided, or navigation is to the same full location
                    if (!targetLoc.Contains("route=") || targetLoc.EndsWith("route=") || (args.Current != null && args.Current.Location.ToString() == targetLoc))
                    {
                        if (CurrentPage is Views.WebContainerPage webPage)
                        {
                            webPage.ReturnToRoot();
                        }
                    }
                }
            }
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
