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

            // Handle Notification Tap safely (Push & Scheduled Reminders)
            DeepLinkBridge.OnNotificationTapped += async (tapData) =>
            {
                await HandleNotificationTapSafelyAsync(tapData);
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
                            bool open = await Khadamat.MobileApp.Helpers.KhadamatDialogHelper.ShowConfirmAsync(
                                reminder.Title,
                                reminder.Message,
                                "انتقال الآن",
                                "لاحقاً",
                                Khadamat.MobileApp.Views.Popups.MobileDialogType.Info,
                                Current.MainPage
                            );
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

    public static async Task HandleNotificationTapSafelyAsync(NotificationTapData tapData)
    {
        if (tapData == null) return;

        // 1. Navigate to specific route if available; otherwise go to notifications list
        if (!string.IsNullOrWhiteSpace(tapData.Route) && tapData.Route != "/" && tapData.Route != "//")
        {
            await NavigateDeepLinkSafelyAsync(tapData.Route);
        }
        else
        {
            await NavigateDeepLinkSafelyAsync("notifications");
        }

        // 2. Display the notification text in an in-app dialog so the user can easily read it
        if (!string.IsNullOrWhiteSpace(tapData.Message) && !string.IsNullOrWhiteSpace(tapData.Title))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Delay slightly to give page time to mount
                    await Task.Delay(800);
                    if (Current?.MainPage != null)
                    {
                        await Khadamat.MobileApp.Helpers.KhadamatDialogHelper.ShowAlertAsync(
                            tapData.Title,
                            tapData.Message,
                            "حسناً",
                            Khadamat.MobileApp.Views.Popups.MobileDialogType.Info,
                            Current.MainPage
                        );
                    }
                }
                catch { }
            });
        }
    }

    public static async Task NavigateDeepLinkSafelyAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        var clean = url.Trim();
        if (clean == "/" || clean == "//") return;

        // Strip scheme / host if full URI
        clean = clean.Replace("khadamat://", "").Replace("https://khadamat.com/", "").TrimStart('/');
        if (string.IsNullOrWhiteSpace(clean)) return;

        // If Shell is not ready yet (e.g. cold start), retry up to 20 times (10 seconds total)
        for (int i = 0; i < 20; i++)
        {
            if (Shell.Current is AppShell appShell)
            {
                await appShell.HandleDeepLink(clean);
                return;
            }
            else if (Shell.Current?.BindingContext is ViewModels.ShellViewModel vm)
            {
                int wait = 0;
                while (vm.IsBusy && wait < 20)
                {
                    await Task.Delay(250);
                    wait++;
                }
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
