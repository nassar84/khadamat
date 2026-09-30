using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Khadamat.Application.DTOs;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Khadamat.Application.Common.Models;

namespace Khadamat.MobileApp.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string appName = "خدماوى";

    [ObservableProperty]
    private string appNameAr = "خدماوى";

    [ObservableProperty]
    private string appNameEn = "Khadamawy";

    [ObservableProperty]
    private bool isBusy = false;

    [ObservableProperty]
    private string currentTab = "home";

    // Sound filenames
    [ObservableProperty]
    private string? openAppSound;
    [ObservableProperty]
    private string? findServiceSound;
    [ObservableProperty]
    private string? openDetailsSound;
    [ObservableProperty]
    private string? messageReceivedSound;
    [ObservableProperty]
    private string? notificationReceivedSound;

    [ObservableProperty]
    private string appLogo = "app_logo.png";

    [ObservableProperty]
    private string userTitle = "دخول";

    [ObservableProperty]
    private string userName = "مستخدم";

    [ObservableProperty]
    private string userImage = "app_logo.png";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    [NotifyPropertyChangedFor(nameof(IsGuest))]
    [NotifyPropertyChangedFor(nameof(IsClient))]
    [NotifyPropertyChangedFor(nameof(IsProviderActive))]
    [NotifyPropertyChangedFor(nameof(IsAdminActive))]
    private bool isAuthenticated = false;

    public bool IsNotAuthenticated => !IsAuthenticated;
    public bool IsGuest => !IsAuthenticated;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsClient))]
    [NotifyPropertyChangedFor(nameof(IsProviderActive))]
    [NotifyPropertyChangedFor(nameof(IsAdminActive))]
    private bool isAdmin = false;

    [ObservableProperty]
    private bool isSuperAdmin = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsClient))]
    [NotifyPropertyChangedFor(nameof(IsProviderActive))]
    private bool isProvider = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsClientMode))]
    [NotifyPropertyChangedFor(nameof(IsClient))]
    [NotifyPropertyChangedFor(nameof(IsProviderActive))]
    private bool isProviderMode = false;

    public bool IsClientMode => !IsProviderMode;

    public bool IsClient => IsAuthenticated && !IsAdmin && (!IsProvider || !IsProviderMode);
    public bool IsProviderActive => IsAuthenticated && !IsAdmin && IsProvider && IsProviderMode;
    public bool IsAdminActive => IsAuthenticated && IsAdmin;

    [RelayCommand]
    private async Task ToggleMode()
    {
        if (!IsProvider) return;
        IsProviderMode = !IsProviderMode;
        Preferences.Default.Set("IsProviderMode", IsProviderMode);
        Shell.Current.FlyoutIsPresented = false;

        // Refresh bottom nav bar and listeners
        AuthChanged?.Invoke(this, EventArgs.Empty);

        // Navigate inside WebView (JS injection) to avoid reloading entire WebView
        string targetRoute = IsProviderMode ? "provider/dashboard" : "";
        try
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage is NavigationPage navPage) currentPage = navPage.CurrentPage;

            if (currentPage is Views.WebContainerPage webPage)
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: ToggleMode - navigating inside WebView to: /{targetRoute}");
                await webPage.NavigateToInternalRoute(targetRoute);
            }
            else
            {
                // Fallback: switch Shell tab (will reload WebView)
                string targetTab = IsProviderMode ? "//Provider_Dashboard" : "//Client_Home";
                await Shell.Current.GoToAsync(targetTab);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error switching mode: {ex.Message}");
        }
    }

    public static event EventHandler? AuthChanged;

    public void SetAuthenticated(bool value, string? name = null, string? image = null, bool admin = false, bool provider = false, bool superAdmin = false)
    {
        bool wasAuthenticated = IsAuthenticated;
        bool wasProvider = IsProvider;

        IsAuthenticated = value;
        IsAdmin = admin;
        IsSuperAdmin = superAdmin;
        IsProvider = provider;
        
        // Providers start in provider mode by default on first login/role change, respecting saved user preference
        if (value && provider && !admin)
        {
            if (!wasAuthenticated || !wasProvider)
            {
                IsProviderMode = Preferences.Default.Get("IsProviderMode", true);
            }
            // If already authenticated and already provider, keep current user-chosen IsProviderMode!
        }
        else
        {
            IsProviderMode = false;
        }
        
        if (value)
        {
            UserName = !string.IsNullOrEmpty(name) ? name : "مستخدم";
            
            // Check if image looks like a real path; if not, use fallback logo instead of a missing file name
            if (!string.IsNullOrEmpty(image) && (image.Contains("/") || image.Contains(".") || image.StartsWith("http") || image.StartsWith("data:")))
                UserImage = ResolveFullImageUrl(image);
            else
                UserImage = "app_logo.png";
                
            UserTitle = (string.IsNullOrEmpty(name) || name.StartsWith("facebook_", StringComparison.OrdinalIgnoreCase) || name.StartsWith("google_", StringComparison.OrdinalIgnoreCase)) ? "حسابي" : name.Split(' ')[0];
        }
        else
        {
            UserName = "زائر";
            UserTitle = "دخول";
            UserImage = "app_logo.png";
            IsAdmin = false;
            IsSuperAdmin = false;
            IsProvider = false;
        }

        AuthChanged?.Invoke(this, EventArgs.Empty);

        // Persist auth state
        var prefs = Microsoft.Maui.Storage.Preferences.Default;
        prefs.Set("IsAuthenticated", value);
        prefs.Set("IsAdmin", admin);
        prefs.Set("IsProvider", provider);
        
        if (value)
        {
            prefs.Set("UserName", UserName);
            prefs.Set("UserImage", UserImage);
        }
        else
        {
            prefs.Remove("UserName");
            prefs.Remove("UserImage");
        }
    }

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ShellViewModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Apply saved theme at startup
        var prefs = Microsoft.Maui.Storage.Preferences.Default;
        string savedTheme = prefs.Get("AppTheme", "default");
        ApplyThemeResources(savedTheme);

        // Restore auth state from preferences
        isAuthenticated = prefs.Get("IsAuthenticated", false);
        isAdmin = prefs.Get("IsAdmin", false);
        isProvider = prefs.Get("IsProvider", false);
        
        if (isAuthenticated)
        {
            userName = prefs.Get("UserName", "مستخدم");
            userImage = ResolveFullImageUrl(prefs.Get("UserImage", "app_logo.png"));
            userTitle = (string.IsNullOrEmpty(userName) || userName.StartsWith("facebook_", StringComparison.OrdinalIgnoreCase) || userName.StartsWith("google_", StringComparison.OrdinalIgnoreCase)) ? "حسابي" : userName.Split(' ')[0];
            
            // Refresh user profile in background to validate token & ensure latest avatar and name
            Task.Run(async () => await RefreshUserProfileAsync());
        }

        // Restore Brand Colors from Cache
        string cachedPrimary = prefs.Get("BrandPrimary", "");
        string cachedSecondary = prefs.Get("BrandSecondary", "");
        if (!string.IsNullOrEmpty(cachedPrimary) && savedTheme == "default")
        {
            try { 
                var res = Microsoft.Maui.Controls.Application.Current.Resources;
                res["Primary"] = Color.FromArgb(cachedPrimary);
                if (!string.IsNullOrEmpty(cachedSecondary)) res["Secondary"] = Color.FromArgb(cachedSecondary);
            } catch { }
        }
    }

    [RelayCommand]
    private async Task Navigate(string route)
    {
        if (string.IsNullOrEmpty(route) || IsBusy) return;
        
        try
        {
            IsBusy = true;
            Console.WriteLine($"ANTIGRAVITY_LOG: Navigating to {route}");

            // Close flyout first
            Shell.Current.FlyoutIsPresented = false;
            
            if (route == "logout")
            {
                if (!IsAuthenticated) return;
                
                try
                {
                    Microsoft.Maui.Storage.SecureStorage.Remove("authToken");
                    Microsoft.Maui.Storage.SecureStorage.Remove("refreshToken");
                }
                catch { }

                try
                {
                    var cPage = Shell.Current.CurrentPage;
                    if (cPage is NavigationPage nPage) cPage = nPage.CurrentPage;
                    
                    if (cPage is Views.WebContainerPage wPage)
                    {
                        await wPage.ForceLogoutInWebView();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ANTIGRAVITY_LOG: Error clearing web auth: {ex.Message}");
                }

                SetAuthenticated(false);
                try
                {
                    await Shell.Current.GoToAsync("//Guest_Home");
                }
                catch
                {
                    // Fallback to reload if needed
                    var cPage = Shell.Current.CurrentPage;
                    if (cPage is NavigationPage nPage) cPage = nPage.CurrentPage;
                    if (cPage is Views.WebContainerPage wPage) wPage.ReturnToRoot();
                }
                return;
            }
            
            // Map the route to the exact Blazor web route path
            string blazorRoute = route;
            
            if (route == "marketplace")
                blazorRoute = "marketplace";
            else if (route == "profile")
                blazorRoute = "profile";
            else if (route == "login")
                blazorRoute = "login";
            else if (route == "register")
                blazorRoute = "register";
            else if (route == "favorites")
                blazorRoute = "client/favorites";
            else if (route == "messages")
                blazorRoute = "messages";
            else if (route == "my-requests" || route == "user/my-requests")
                blazorRoute = "user/my-requests";
            else if (route == "provider/incoming-requests" || route == "incoming-requests")
                blazorRoute = "provider/incoming-requests";
            else if (route == "provider/dashboard")
                blazorRoute = "provider/dashboard";
            else if (route == "my-services" || route == "provider/my-services" || route == "provider/services")
                blazorRoute = "provider/my-services";
            else if (route == "provider/create-request" || route == "add-service" || route == "provider/apply")
                blazorRoute = "provider/create-request";
            else if (route == "provider/subscription")
                blazorRoute = "provider/subscription";
            else if (route == "services")
                blazorRoute = "services";
            else if (route == "settings")
                blazorRoute = "settings";
            else if (route == "admin" || route == "admin/dashboard")
                blazorRoute = "admin";
            else if (route == "admin/approvals" || route == "approvals")
                blazorRoute = "admin/approvals";
            else if (route == "admin/complaints" || route == "complaints")
                blazorRoute = "admin/complaints";
            else if (route == "admin/ads")
                blazorRoute = "admin/ads";
            else if (route == "admin/users")
                blazorRoute = "admin/users";
            else if (route == "admin/categories")
                blazorRoute = "admin/categories";
            else if (route == "admin/services")
                blazorRoute = "admin/services";
            else if (route == "admin/governorates" || route == "admin/locations")
                blazorRoute = "admin/locations";
            else if (route == "admin/settings" || route == "admin/systemsettings")
                blazorRoute = "admin/settings";
            else if (route == "feed")
                blazorRoute = "feed";
            else if (route == "terms")
                blazorRoute = "terms";
            else if (route == "privacy")
                blazorRoute = "privacy";
            else if (route == "home" || route == "//HomePage")
                blazorRoute = "";
            else if (route == "categories")
                blazorRoute = "categories";
            else if (route == "explore")
                blazorRoute = "explore";
            else if (route == "search")
                blazorRoute = "search";
            else if (route == "support" || route == "contact")
                blazorRoute = "contact";
            else if (route == "notifications")
                blazorRoute = "notifications";
            else if (!route.StartsWith("//"))
                blazorRoute = route;
            else if (route.StartsWith("//"))
                blazorRoute = ""; // Fallback

            // Update CurrentTab for UI Highlighting enthusiastically
            var lowerRoute = blazorRoute.ToLower();
            if (lowerRoute.Contains("categories")) CurrentTab = "categories";
            else if (lowerRoute.Contains("services") && !lowerRoute.Contains("my-services")) CurrentTab = "services";
            else if (lowerRoute.Contains("favorite") || lowerRoute.Contains("my-services") || lowerRoute.Contains("provider/services") || lowerRoute.Contains("provider/dashboard")) CurrentTab = "favorites";
            else if (lowerRoute.Contains("messages")) CurrentTab = "messages";
            else if (lowerRoute.Contains("profile") || lowerRoute.Contains("login") || lowerRoute.Contains("register")) CurrentTab = "profile";
            else if (lowerRoute.Contains("marketplace")) CurrentTab = "marketplace";
            else if (lowerRoute.Contains("feed")) CurrentTab = "feed";
            else if (lowerRoute == "" || lowerRoute.Contains("home")) CurrentTab = "home";

            // Identify if we can do an inject-based navigation instead of MAUI shell push
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage is NavigationPage navPage) currentPage = navPage.CurrentPage;
            
            if (currentPage is Views.WebContainerPage webPage)
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: Routing inside WebView to: /{blazorRoute}");
                await webPage.NavigateToInternalRoute(blazorRoute);
            }
            else
            {
                // Fallback to native Home page load if we aren't already in one
                string routeParams = string.IsNullOrEmpty(blazorRoute) ? "" : $"?route={Uri.EscapeDataString(blazorRoute)}";
                await Shell.Current.GoToAsync($"//HomePage{routeParams}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Navigation Error for {route}: {ex.Message}");
            await Shell.Current.GoToAsync("//HomePage");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShowThemePicker()
    {
        // Require CommunityToolkit.Maui.Views
        var popup = new Views.Popups.ThemePickerPopup();
        
        // Show Popup and get result
        var result = await CommunityToolkit.Maui.Views.PopupExtensions.ShowPopupAsync(
            Shell.Current.CurrentPage, 
            popup);

        string? theme = result as string;

        if (theme != null)
        {
            Microsoft.Maui.Storage.Preferences.Default.Set("AppTheme", theme);
            ApplyThemeResources(theme);
            
            // If we are currently on a web container, tell it to update JS immediately
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage is NavigationPage navPage) currentPage = navPage.CurrentPage;
            
            if (currentPage is Views.WebContainerPage webPage)
            {
                await webPage.ApplyThemeToWebView(theme);
            }
        }
    }

    [RelayCommand]
    private async Task ShareApp()
    {
        try
        {
            var prefs = Microsoft.Maui.Storage.Preferences.Default;
            var shareUrl = prefs.Get("AppShareUrl", "https://khadamawy.eis-dev.com/downloads/Khadamawy.apk");
            var customShareText = prefs.Get("AppShareText", "");

            var appDisplayName = string.IsNullOrEmpty(AppNameAr) ? AppName : AppNameAr;
            var text = !string.IsNullOrWhiteSpace(customShareText)
                ? $"{customShareText}\n\n🔗 {shareUrl}"
                : $"📲 تطبيق {appDisplayName} — خدماتك في مكان واحد.. لكل المصريين!\n\n" +
                  "تواصل مباشرة مع أفضل الحرفيين والمهنيين ومقدمي الخدمات بكل سهولة وأمان.\n\n" +
                  $"🔗 رابط تحميل التطبيق:\n{shareUrl}\n\n" +
                  "🌐 أو تصفح الموقع مباشرة:\nhttps://khadamawy.eis-dev.com";

            string title = $"مشاركة تطبيق {appDisplayName}";

            string? shareImagePath = null;
            try
            {
                var cacheDir = FileSystem.CacheDirectory;
                var targetPath = Path.Combine(cacheDir, "khadamat_promo.png");
                
                if (!File.Exists(targetPath))
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync("welcome_hero.png");
                    using var fileStream = File.Create(targetPath);
                    await stream.CopyToAsync(fileStream);
                }
                
                if (File.Exists(targetPath))
                {
                    shareImagePath = targetPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: Could not extract promo image for sharing: {ex.Message}");
            }

            // Always share text + link (image is extra context only, not a replacement)
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = text,
                Title = title,
                Uri = shareUrl
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: ShareApp error: {ex.Message}");
        }
    }

    private void ApplyThemeResources(string theme)
    {
        // Hex codes from khadamat.css
        var colors = theme.ToLower() switch
        {
            "sunset" => ("#ff4e50", "#f9d423"),
            "ocean" => ("#00c6ff", "#0072ff"),
            "forest" => ("#00f260", "#0575e6"),
            "lavender" => ("#bf5af2", "#5e5ce6"),
            "royal" => ("#eab308", "#ca8a04"),
            _ => ("#6366f1", "#f43f5e") // Aurora / Default
        };

        if (Microsoft.Maui.Controls.Application.Current != null)
        {
            var res = Microsoft.Maui.Controls.Application.Current.Resources;
            res["Primary"] = Color.FromArgb(colors.Item1);
            res["Secondary"] = Color.FromArgb(colors.Item2);
            
            // Update TabBar and Header specifically if needed (handled via DynamicResource in XAML)
            Console.WriteLine($"ANTIGRAVITY_LOG: Global Theme Applied: {theme} (Primary: {colors.Item1})");
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            // Close the flyout menu
            Shell.Current.FlyoutIsPresented = false;

            // Find the current page
            var currentPage = Shell.Current.CurrentPage;
            
            // If it's a NavigationPage, we need the current page inside it
            if (currentPage is NavigationPage navPage)
            {
                currentPage = navPage.CurrentPage;
            }

            if (currentPage is Views.WebContainerPage webPage)
            {
                Console.WriteLine("ANTIGRAVITY_LOG: Refreshing App WebView via Native Shell Command");
                webPage.RefreshWebView();
            }
            else
            {
                 Console.WriteLine($"ANTIGRAVITY_LOG: Current page is not WebContainerPage. It is {currentPage?.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error during Refresh command: {ex.Message}");
        }
        
        await Task.CompletedTask;
    }

    [ObservableProperty]
    private string appSlogan = "بوابتك لأفضل الخدمات المهنية";

    public async Task LoadSettingsAsync()
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://10.0.2.2:5144";
            
            // Fetch settings from API
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<AppSettingsDto>>($"{baseUrl.TrimEnd('/')}/v1/settings");
            
            if (response?.Success == true && response.Data != null)
            {
                AppName = response.Data.ApplicationName;
                AppNameAr = response.Data.ApplicationNameAr;
                AppNameEn = response.Data.ApplicationNameEn;
                
                OpenAppSound = response.Data.OpenAppSound;
                FindServiceSound = response.Data.FindServiceSound;
                OpenDetailsSound = response.Data.OpenDetailsSound;
                MessageReceivedSound = response.Data.MessageReceivedSound;
                NotificationReceivedSound = response.Data.NotificationReceivedSound;

                if (!string.IsNullOrEmpty(response.Data.WelcomeMessage))
                {
                    AppSlogan = response.Data.WelcomeMessage;
                }

                if (!string.IsNullOrEmpty(response.Data.PrimaryColor) && Microsoft.Maui.Controls.Application.Current != null)
                {
                    try
                    {
                        var primaryColor = Color.FromArgb(response.Data.PrimaryColor);
                        Microsoft.Maui.Controls.Application.Current.Resources["Primary"] = primaryColor;
                        
                        if (!string.IsNullOrEmpty(response.Data.SecondaryColor))
                        {
                            var secondaryColor = Color.FromArgb(response.Data.SecondaryColor);
                            Microsoft.Maui.Controls.Application.Current.Resources["Secondary"] = secondaryColor;
                        }

                        // Save to cache for next startup
                        var p = Microsoft.Maui.Storage.Preferences.Default;
                        p.Set("BrandPrimary", response.Data.PrimaryColor);
                        p.Set("BrandSecondary", response.Data.SecondaryColor);
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(response.Data.LogoUrl))
                {
                    // Ensure full URL for the image
                    if (response.Data.LogoUrl.StartsWith("http"))
                    {
                        AppLogo = response.Data.LogoUrl;
                    }
                    else
                    {
                        AppLogo = $"{baseUrl.TrimEnd('/')}/{response.Data.LogoUrl.TrimStart('/')}";
                    }
                }

                // Cache share settings for native share
                var prefStorage = Microsoft.Maui.Storage.Preferences.Default;
                if (!string.IsNullOrEmpty(response.Data.AppShareUrl))
                {
                    prefStorage.Set("AppShareUrl", response.Data.AppShareUrl);
                }
                if (!string.IsNullOrEmpty(response.Data.AppShareText))
                {
                    prefStorage.Set("AppShareText", response.Data.AppShareText);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error loading settings in ShellViewModel: {ex.Message}");
        }
    }

    public async Task RefreshUserProfileAsync()
    {
        try
        {
            string? token = null;
            try
            {
                token = await Microsoft.Maui.Storage.SecureStorage.GetAsync("authToken");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: SecureStorage error reading token: {ex.Message}");
            }

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("ANTIGRAVITY_LOG: No valid token found in SecureStorage. Clearing native auth state.");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetAuthenticated(false);
                });
                return;
            }

            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? 
                          Microsoft.Maui.Storage.Preferences.Default.Get("ApiBaseUrl", "https://khadamawy.eis-dev.com");
            baseUrl = baseUrl.TrimEnd('/');

            using var req = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/v1/auth/profile");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var res = await _httpClient.SendAsync(req);
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<Khadamat.Application.Common.Models.ApiResponse<Khadamat.Application.DTOs.AuthResponse>>();
                if (content?.Success == true && content.Data != null)
                {
                    var p = content.Data;
                    var isAdmin = p.Roles.Any(r => r == "SystemAdmin" || r == "SuperAdmin");
                    var isSuper = p.Roles.Any(r => r == "SuperAdmin");
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SetAuthenticated(true, p.UserName, p.ImageUrl, isAdmin, p.IsProvider, isSuper);
                    });
                }
            }
            else if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized || res.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: Profile request returned {res.StatusCode}. Logging out native shell.");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetAuthenticated(false);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error fetching user profile in ShellViewModel: {ex.Message}");
        }
    }

    public static string ResolveFullImageUrl(string? image, string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(image))
            return "app_logo.png";

        var img = image.Trim();

        if (img == "app_logo.png" || img == "profile_icon.png")
            return img;

        if (img.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            return img;

        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = Microsoft.Maui.Storage.Preferences.Default.Get("ApiBaseUrl", "https://khadamawy.eis-dev.com");
        }
        baseUrl = baseUrl.TrimEnd('/');

        string fullUrl;
        if (img.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            img.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            fullUrl = img;
        }
        else
        {
            img = img.TrimStart('/');
            if (!img.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            {
                img = $"images/users/{img}";
            }
            fullUrl = $"{baseUrl}/{img}";
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return fullUrl.Contains("?") ? $"{fullUrl}&t={timestamp}" : $"{fullUrl}?t={timestamp}";
    }
}
