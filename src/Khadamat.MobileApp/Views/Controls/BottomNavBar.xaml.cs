namespace Khadamat.MobileApp.Views.Controls;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
        
        Loaded += (s, e) =>
        {
            EnsureViewModel();
            RefreshAuthState();
        };

        // Subscribe to global auth changes to keep UI in sync
        ViewModels.ShellViewModel.AuthChanged += OnAuthChanged;

        Unloaded += (s, e) => {
            ViewModels.ShellViewModel.AuthChanged -= OnAuthChanged;
        };
    }

    private ViewModels.ShellViewModel? EnsureViewModel()
    {
        if (BindingContext is ViewModels.ShellViewModel vm)
            return vm;

        if (Shell.Current?.BindingContext is ViewModels.ShellViewModel shellVm)
        {
            BindingContext = shellVm;
            return shellVm;
        }

        try
        {
            var sp = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
            var resolvedVm = sp?.GetService<ViewModels.ShellViewModel>();
            if (resolvedVm != null)
            {
                BindingContext = resolvedVm;
                return resolvedVm;
            }
        }
        catch { }

        return null;
    }

    private void OnAuthChanged(object? sender, EventArgs e)
    {
        RefreshAuthState();
    }

    // ─── Auth State ─────────────────────────────────────────────────────────
    public void RefreshAuthState()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var vm = EnsureViewModel();
                if (vm == null) return;

                bool hasImage = vm.IsAuthenticated &&
                                !string.IsNullOrEmpty(vm.UserImage) && 
                                vm.UserImage != "profile_icon.png" && 
                                vm.UserImage != "app_logo.png" &&
                                !vm.UserImage.ToLower().Contains("default");

                if (ProfileIcon != null) ProfileIcon.IsVisible = !hasImage;
                if (ProfileImageBorder != null) ProfileImageBorder.IsVisible = hasImage;

                if (hasImage && ProfileImage != null)
                {
                    if (vm.UserImage.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                        vm.UserImage.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        ProfileImage.Source = new UriImageSource
                        {
                            Uri = new Uri(vm.UserImage),
                            CachingEnabled = false
                        };
                    }
                    else if (vm.UserImage.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                    {
                        ProfileImage.Source = vm.UserImage;
                    }
                    else
                    {
                        var fullUrl = ViewModels.ShellViewModel.ResolveFullImageUrl(vm.UserImage);
                        ProfileImage.Source = new UriImageSource
                        {
                            Uri = new Uri(fullUrl),
                            CachingEnabled = false
                        };
                    }
                }
                else if (ProfileImage != null)
                {
                    ProfileImage.Source = null;
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
        // Tab tracking is now handled via DataTrigger bindings in XAML
    }

    // ─── Tap Handlers ────────────────────────────────────────────────────────

    private void OnHomeTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is ViewModels.ShellViewModel vm)
        {
            vm.NavigateCommand.Execute("home");
            _ = AnimateCircleAsync(GuestHomeCircle);
        }
    }

    // ─── Animations ──────────────────────────────────────────────────────────

    private async Task AnimateCircleAsync(Border circle)
    {
        try
        {
            await circle.ScaleTo(0.88, 80, Easing.CubicOut);
            await circle.ScaleTo(1.0, 140, Easing.BounceOut);
        }
        catch { }
    }
}
