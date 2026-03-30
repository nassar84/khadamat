namespace Khadamat.MobileApp.Views.Controls;

public partial class BottomNavBar : ContentView
{
    private string _activeTab = "home";

    public BottomNavBar()
    {
        InitializeComponent();
    }

    // ─── Auth State ──────────────────────────────────────────────────────────

    /// <summary>
    /// Call this whenever auth state changes to update the profile label/icon.
    /// </summary>
    public void RefreshAuthState()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var vm = Shell.Current?.BindingContext as ViewModels.ShellViewModel;
                if (vm == null) return;

                if (vm.IsAuthenticated)
                {
                    // Show first word of user's name (matches website behaviour)
                    var title = vm.UserTitle;
                    ProfileLabel.Text = string.IsNullOrWhiteSpace(title) ? "حسابي" : title.Split(' ')[0];
                    ProfileIcon.Text = "\U0001F464"; // 👤  person silhouette
                }
                else
                {
                    ProfileLabel.Text = "دخول";
                    ProfileIcon.Text = "\U0001F464";
                }
            }
            catch { /* ignore if Shell not ready */ }
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
        var defaultColor = Color.FromArgb("#94a3b8");

        // Reset all non-home labels to default grey
        CategoriesLabel.TextColor  = defaultColor;
        FavoritesLabel.TextColor   = defaultColor;
        ProfileLabel.TextColor     = defaultColor;
        
        // Marketplace uses distinct premium color on web
        MarketplaceIcon.TextColor = Color.FromArgb("#f59e0b");
        MarketplaceLabel.TextColor = Color.FromArgb("#d97706");

        // "home" is always styled via the circle button; no label change needed
        switch (_activeTab)
        {
            case "categories":
                CategoriesLabel.TextColor = primaryColor;
                break;
            case "marketplace":
                MarketplaceLabel.TextColor = primaryColor;
                break;
            case "favorites":
                FavoritesLabel.TextColor = primaryColor;
                break;
            case "profile":
                ProfileLabel.TextColor = primaryColor;
                break;
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

    private async void OnHomeTapped(object sender, TappedEventArgs e)
    {
        SetActiveTab("home");
        await AnimateHomeCircleAsync();
        await Shell.Current.GoToAsync("//HomePage");
    }

    private async void OnMarketplaceTapped(object sender, TappedEventArgs e)
    {
        SetActiveTab("marketplace");
        await Shell.Current.GoToAsync("//MarketplacePage");
    }

    private async void OnFavoritesTapped(object sender, TappedEventArgs e)
    {
        var vm = Shell.Current?.BindingContext as ViewModels.ShellViewModel;
        if (vm?.IsAuthenticated == false)
        {
            // Redirect unauthenticated users to login
            await Shell.Current.GoToAsync("//ProfileTab");
            return;
        }
        SetActiveTab("favorites");
        await Shell.Current.GoToAsync("//FavoritesPage");
    }

    private async void OnCategoriesTapped(object sender, TappedEventArgs e)
    {
        SetActiveTab("categories");
        await Shell.Current.GoToAsync("//CategoriesPage");
    }

    private async void OnProfileTapped(object sender, TappedEventArgs e)
    {
        SetActiveTab("profile");
        await Shell.Current.GoToAsync("//ProfileTab");
    }

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
