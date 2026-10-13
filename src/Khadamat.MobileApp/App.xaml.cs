using Khadamat.MobileApp.Services;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace Khadamat.MobileApp;

public partial class App : MauiApp
{
    private readonly Views.WelcomePage _welcomePage;

    public App(Views.WelcomePage welcomePage, ILocalReminderScheduler reminderScheduler)
    {
        try 
        {
            InitializeComponent();
            _welcomePage = welcomePage;

            // Simple safe theme initialization
            string savedTheme = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", "default");
            ApplyInitialTheme(savedTheme);

            // Handle DeepLink safely
            DeepLinkBridge.OnDeepLinkReceived += async (url) => 
            {
                await NavigateDeepLinkSafelyAsync(url);
            };

            // In-app reminder dialog handler
            reminderScheduler.OnInAppReminderDue += (reminder) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        if (Current?.MainPage != null)
                        {
                            bool open = await Current.MainPage.DisplayAlert(reminder.Title, reminder.Message, "انتقال الآن", "لاحقاً");
                            reminderScheduler.MarkInAppReminderSeen(reminder.Id);
                            if (open && !string.IsNullOrEmpty(reminder.ActionRoute))
                            {
                                await NavigateDeepLinkSafelyAsync(reminder.ActionRoute);
                            }
                        }
                    }
                    catch { }
                });
            };

            // Initialize local reminders and sync schedule in background
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000); // Allow app bootstrap to complete first
                await reminderScheduler.InitializeAsync();
            });

            // Register FCM device token with backend API in background with retry
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    try
                    {
                        using var scope = IPlatformApplication.Current?.Services?.CreateScope();
                        var fcmService = scope?.ServiceProvider.GetService<FcmTokenRegistrationService>();
                        if (fcmService != null)
                        {
                            await fcmService.RegisterTokenAsync();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[FCM Boot] Attempt {attempt} error: {ex.Message}");
                    }
                    await Task.Delay(7000);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: [CRITICAL APP INIT ERROR] {ex.Message}");
        }
    }

    private void ApplyInitialTheme(string theme)
    {
        try 
        {
            string primaryHex = theme switch
            {
                "sunset" => "#ea580c",
                "ocean" => "#0284c7",
                "forest" => "#16a34a",
                "lavender" => "#9333ea",
                "royal" => "#db2777",
                _ => "#6366f1"
            };
            Resources["Primary"] = Color.FromArgb(primaryHex);
        }
        catch { }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Page initialPage;
        bool hasCompleted = Microsoft.Maui.Storage.Preferences.Default.Get("HasCompletedOnboarding", false);
        
        if (hasCompleted)
        {
            // By-pass the Welcome page if they have already opened the app before
            initialPage = _welcomePage.AppShellInstance;
        }
        else
        {
            initialPage = _welcomePage;
        }

        var window = new Window(initialPage) { Title = "Khadamat" };
        window.FlowDirection = FlowDirection.RightToLeft;
        return window;
    }

    public static async Task NavigateDeepLinkSafelyAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        var clean = url.Trim();
        if (clean == "/" || clean == "//") return;

        // Strip scheme / host if full URI
        clean = clean.Replace("khadamat://", "").Replace("https://khadamat.com/", "").TrimStart('/');
        if (string.IsNullOrWhiteSpace(clean)) return;

        // If Shell is not ready yet (e.g. cold start), retry up to 10 times (5 seconds total)
        for (int i = 0; i < 10; i++)
        {
            if (Shell.Current is AppShell appShell)
            {
                await appShell.HandleDeepLink(clean);
                return;
            }
            else if (Shell.Current?.BindingContext is ViewModels.ShellViewModel vm)
            {
                await vm.NavigateCommand.ExecuteAsync(clean);
                return;
            }
            await Task.Delay(500);
        }

        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync($"//HomePage?route={Uri.EscapeDataString(clean)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[App] Deep link navigation failed: {ex.Message}");
        }
    }
}
