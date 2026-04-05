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
                    // Show first word of user's name — matches web "حسابي" behaviour
                    var title = vm.UserTitle;
                    ProfileLabel.Text = string.IsNullOrWhiteSpace(title) ? "حسابي" : title.Split(' ')[0];

                    // Profile Image Logic: Show image if it exists and looks like a valid path/url
                    bool hasImage = !string.IsNullOrEmpty(vm.UserImage) && 
                                    vm.UserImage != "profile_icon.png" && 
                                    !vm.UserImage.ToLower().Contains("default") &&
                                    (vm.UserImage.EndsWith(".png") || vm.UserImage.EndsWith(".jpg") || vm.UserImage.EndsWith(".jpeg") || vm.UserImage.Contains("/profiles/"));

                    if (hasImage)
                    {
                        ProfileIcon.IsVisible = false;
                        ProfileImageBorder.IsVisible = true;
                        
                        // Handle relative vs absolute URLs (Fetch baseUrl from settings)
                        if (vm.UserImage.StartsWith("http") || vm.UserImage.StartsWith("https"))
                        {
                            ProfileImage.Source = vm.UserImage;
                        }
                        else
                        {
                            // Use the same base URL used for settings
                            string baseUrl = Microsoft.Maui.Storage.Preferences.Default.Get("WebAppBaseUrl", "https://jobsek.eis-dev.com");
                            ProfileImage.Source = $"{baseUrl.TrimEnd('/')}/{vm.UserImage.TrimStart('/')}";
                            
                            Console.WriteLine($"ANTIGRAVITY_LOG: Setting User Image Source: {ProfileImage.Source}");
                        }
                    }
                    else
                    {
                        ProfileIcon.IsVisible = true;
                        ProfileImageBorder.IsVisible = false;
                        // fa-circle-user (FA6Solid \uf2bd) — indicates logged-in state
                        ProfileIcon.Text = "\uf2bd";
                        ProfileIcon.FontFamily = "FA6Solid";
                    }
                }
                else
                {
                    ProfileIcon.IsVisible = true;
                    ProfileImageBorder.IsVisible = false;
                    ProfileLabel.Text = "دخول";
                    // fa-user (FA6Solid \uf007) — default login icon
                    ProfileIcon.Text = "\uf007";
                    ProfileIcon.FontFamily = "FA6Solid";
                }
            }
            catch (Exception ex) 
            { 
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

    private async void OnHomeTapped(object sender, TappedEventArgs e)
    {
        try 
        {
            SetActiveTab("home");
            await AnimateHomeCircleAsync();
            await Shell.Current.GoToAsync("//HomePage");
        }
        catch { }
    }

    private async void OnMarketplaceTapped(object sender, TappedEventArgs e)
    {
        try
        {
            SetActiveTab("marketplace");
            await Shell.Current.GoToAsync("//MarketplacePage");
        }
        catch { }
    }

    private async void OnFavoritesTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var vm = Shell.Current?.BindingContext as ViewModels.ShellViewModel;
            if (vm?.IsAuthenticated == false)
            {
                await Shell.Current.GoToAsync("//ProfileTab");
                return;
            }
            SetActiveTab("favorites");
            await Shell.Current.GoToAsync("//FavoritesPage");
        }
        catch { }
    }

    private async void OnMessagesTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var vm = Shell.Current?.BindingContext as ViewModels.ShellViewModel;
            if (vm?.IsAuthenticated == false)
            {
                await Shell.Current.GoToAsync("//ProfileTab");
                return;
            }
            SetActiveTab("messages");
            // Fix: Use the generic route to avoid crashes if MessagesPage is not in the Shell root stack
            await Shell.Current.GoToAsync("//HomePage?route=messages");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Messages Nav Error: {ex.Message}");
        }
    }

    private async void OnProfileTapped(object sender, TappedEventArgs e)
    {
        try
        {
            SetActiveTab("profile");
            await Shell.Current.GoToAsync("//ProfileTab");
        }
        catch { }
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
