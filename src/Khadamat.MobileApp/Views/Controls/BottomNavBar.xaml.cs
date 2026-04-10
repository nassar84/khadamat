namespace Khadamat.MobileApp.Views.Controls;

public partial class BottomNavBar : ContentView
{
    private string _activeTab = "home";

    public BottomNavBar()
    {
        InitializeComponent();
        
        // Subscribe to global auth changes to keep UI in sync
        ViewModels.ShellViewModel.AuthChanged += OnAuthChanged;

        Unloaded += (s, e) => {
            ViewModels.ShellViewModel.AuthChanged -= OnAuthChanged;
        };
    }

    private void OnAuthChanged(object? sender, EventArgs e)
    {
        RefreshAuthState();
    }

    // ─── Auth State ─────────────────────────────────────────────────────────    public void RefreshAuthState()
    {
        // Now partially handled by bindings in XAML (ProfileLabel)
        // Manual sync for legacy ProfileImage logic
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var vm = BindingContext as ViewModels.ShellViewModel;
                if (vm == null) return;

                bool hasImage = !string.IsNullOrEmpty(vm.UserImage) && 
                                vm.UserImage != "profile_icon.png" && 
                                vm.UserImage != "app_logo.png" &&
                                !vm.UserImage.ToLower().Contains("default");

                ProfileIcon.IsVisible = !hasImage;
                ProfileImageBorder.IsVisible = hasImage;

                if (hasImage)
                {
                    if (vm.UserImage.StartsWith("http") || vm.UserImage.StartsWith("https") || vm.UserImage.StartsWith("data:image"))
                    {
                        ProfileImage.Source = vm.UserImage;
                    }
                    else
                    {
                        string baseUrl = Microsoft.Maui.Storage.Preferences.Default.Get("WebAppBaseUrl", "https://khadamat.com");
                        ProfileImage.Source = $"{baseUrl.TrimEnd('/')}/{vm.UserImage.TrimStart('/')}";
                    }
                }
            }
            catch { }
        });
    }            { 
                Console.WriteLine($"ANTIGRAVITY_LOG: Error in RefreshAuthState: {ex.Message}");
            }
        });
    }

    // ─── Active Tab Tracking ─────────────────────────────────────────────────

    public void SetActiveTab(string tab)
    {
        _activeTab = tab;
        UpdateActiveColors();
    }

    private void UpdateActiveColors()
    {
        var primaryColor = GetPrimaryColor();
        // Matches web: .nav-item { color: #475569 } (inactive)
        var defaultColor = Color.FromArgb("#475569");

        // Reset ALL items to default gray (icon + label) — same as web default state
        MarketplaceRightIcon.TextColor  = defaultColor;
        MarketplaceRightLabel.TextColor = defaultColor;
        FavoritesRightIcon.TextColor    = defaultColor;
        FavoritesRightLabel.TextColor   = defaultColor;
        MessagesIcon.TextColor          = defaultColor;
        MessagesLabel.TextColor         = defaultColor;
        ProfileIcon.TextColor           = defaultColor;
        ProfileLabel.TextColor          = defaultColor;
        ProfileImageBorder.Stroke       = Colors.Transparent;
        ProfileImageBorder.StrokeThickness = 0;

        // Apply primary purple to active tab (icon + label) — matches web .nav-item.active
        switch (_activeTab)
        {
            case "marketplace":
                MarketplaceRightIcon.TextColor  = primaryColor;
                MarketplaceRightLabel.TextColor = primaryColor;
                break;
            case "favorites":
                FavoritesRightIcon.TextColor  = primaryColor;
                FavoritesRightLabel.TextColor = primaryColor;
                break;
            case "messages":
                MessagesIcon.TextColor  = primaryColor;
                MessagesLabel.TextColor = primaryColor;
                break;
            case "profile":
                ProfileIcon.TextColor  = primaryColor;
                ProfileLabel.TextColor = primaryColor;
                ProfileImageBorder.Stroke = primaryColor;
                ProfileImageBorder.StrokeThickness = 2;
                break;
            // "home" is always styled by the circle + HomeLabel; no extra change needed
        }
    }

    private static Color GetPrimaryColor()
    {
        try
        {
            if (Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue("Primary", out var c) == true && c is Color color)
                return color;
        }
        catch { }
        return Color.FromArgb("#6366f1");
    }

    // ─── Tap Handlers ────────────────────────────────────────────────────────

    private void OnHomeTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is ViewModels.ShellViewModel vm)
        {
            vm.NavigateCommand.Execute("home");
            _ = AnimateHomeCircleAsync();
        }
    }

    // Handlers for side items are now handled via Command bindings in XAML, 
    // but we can keep these as empty or remove if we remove their Tapped attributes in XAML.
    // I already removed Tapped attributes in XAML for side items.

    // ─── Animations ──────────────────────────────────────────────────────────

    private async Task AnimateHomeCircleAsync()
    {
        try
        {
            await HomeCircle.ScaleTo(0.88, 80, Easing.CubicOut);
            await HomeCircle.ScaleTo(1.0,  140, Easing.BounceOut);
        }
        catch { }
    }
}
