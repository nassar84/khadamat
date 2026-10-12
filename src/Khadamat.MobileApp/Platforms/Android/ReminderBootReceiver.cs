#if ANDROID
using System;
using Android.App;
using Android.Content;
using Khadamat.Domain.Enums;
using Khadamat.MobileApp.Services;
using Microsoft.Maui.Storage;

namespace Khadamat.MobileApp.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { 
    Intent.ActionBootCompleted, 
    Intent.ActionMyPackageReplaced,
    "android.intent.action.QUICKBOOT_POWERON", 
    "com.htc.intent.action.QUICKBOOT_POWERON" 
})]
public class ReminderBootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        var action = intent.Action;
        if (action == Intent.ActionBootCompleted ||
            action == Intent.ActionMyPackageReplaced ||
            action == "android.intent.action.QUICKBOOT_POWERON" ||
            action == "com.htc.intent.action.QUICKBOOT_POWERON")
        {
            Console.WriteLine($"[ReminderBootReceiver] Device event received ({action}). Re-scheduling active reminders...");

            try
            {
                if (!Preferences.Default.Get("RemindersEnabled", true))
                {
                    Console.WriteLine("[ReminderBootReceiver] Reminders are disabled. Skipping.");
                    return;
                }

                var cachedReminders = LocalReminderScheduler.GetCachedReminders();
                if (cachedReminders == null || cachedReminders.Count == 0)
                {
                    Console.WriteLine("[ReminderBootReceiver] No cached reminders found.");
                    return;
                }

                var scheduler = new AndroidReminderScheduler();
                scheduler.Initialize();

                foreach (var reminder in cachedReminders)
                {
                    if (reminder.DisplayType == ReminderDisplayType.NotificationBar)
                    {
                        scheduler.ScheduleNotification(reminder);
                    }
                }

                Console.WriteLine($"[ReminderBootReceiver] Successfully re-scheduled {cachedReminders.Count} reminders after boot.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReminderBootReceiver] Error rescheduling reminders: {ex.Message}");
            }
        }
    }
}
#endif
