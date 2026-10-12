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

            // Handle DeepLink
            DeepLinkBridge.OnDeepLinkReceived += async (url) => 
            {
                if (Microsoft.Maui.Controls.Application.Current != null && Shell.Current != null)
                {
                    await Shell.Current.GoToAsync(url);
                }
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
                                DeepLinkBridge.Raise(reminder.ActionRoute);
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

            // Register FCM device token with backend API in background
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                try
                {
                    using var scope = IPlatformApplication.Current?.Services?.CreateScope();
                    var fcmService = scope?.ServiceProvider.GetService<FcmTokenRegistrationService>();
                    if (fcmService != null)
                    {
                        await fcmService.RegisterTokenAsync();
                    }
                }
                catch { }
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
}
