#if ANDROID
using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Khadamat.Domain.Enums;
using Microsoft.Maui.Storage;

namespace Khadamat.MobileApp.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class ReminderAlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        // Check user privacy / opt-out setting
        if (!Preferences.Default.Get("RemindersEnabled", true))
        {
            Console.WriteLine("[ReminderAlarmReceiver] Reminders are disabled by user. Ignoring.");
            return;
        }

        var id = intent.GetIntExtra("reminder_id", 1001);
        var title = intent.GetStringExtra("title") ?? "خدماوى";
        var message = intent.GetStringExtra("message") ?? "";
        var actionRoute = intent.GetStringExtra("action_route") ?? "/";
        var recurrence = (ReminderRecurrenceType)intent.GetIntExtra("recurrence_type", (int)ReminderRecurrenceType.Daily);
        var ticks = intent.GetLongExtra("scheduled_time_ticks", 0);
        var intervalVal = intent.GetIntExtra("interval_value", 1);

        Console.WriteLine($"[ReminderAlarmReceiver] Firing reminder ID {id}: '{title}'");

        ShowNotification(context, id, title, message, actionRoute);

        // Schedule next occurrence if recurring
        ScheduleNextOccurrence(context, intent, id, recurrence, ticks, intervalVal);
    }

    private void ShowNotification(Context context, int id, string title, string message, string actionRoute)
    {
        var notificationManager = (NotificationManager?)context.GetSystemService(global::Android.Content.Context.NotificationService);
        if (notificationManager == null) return;

        // Ensure channel exists
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var existingChannel = notificationManager.GetNotificationChannel(AndroidReminderScheduler.ChannelId);
            if (existingChannel == null)
            {
                var channel = new NotificationChannel(
                    AndroidReminderScheduler.ChannelId, 
                    AndroidReminderScheduler.ChannelName, 
                    NotificationImportance.High)
                {
                    Description = AndroidReminderScheduler.ChannelDescription
                };
                channel.EnableLights(true);
                channel.LightColor = global::Android.Graphics.Color.ParseColor("#6366f1");
                channel.EnableVibration(true);
                channel.SetShowBadge(true);
                channel.LockscreenVisibility = NotificationVisibility.Public;
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        // Intent to launch MainActivity with deep link route
        var mainIntent = new Intent(context, Java.Lang.Class.FromType(typeof(MainActivity)));
        mainIntent.SetPackage(context.PackageName);
        mainIntent.SetAction("com.nassar84.khadamawy.NOTIFICATION_OPEN");
        mainIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        mainIntent.PutExtra("from_reminder", true);
        mainIntent.PutExtra("reminder_id", id);
        mainIntent.PutExtra("notification_title", title);
        mainIntent.PutExtra("title", title);
        mainIntent.PutExtra("notification_message", message);
        mainIntent.PutExtra("body", message);

        string effectiveRoute = !string.IsNullOrWhiteSpace(actionRoute) && actionRoute != "/" && actionRoute != "//"
            ? actionRoute
            : "notifications";
        mainIntent.PutExtra("action_route", effectiveRoute);
        mainIntent.PutExtra("navigate_to", effectiveRoute);

        var pendingIntent = PendingIntent.GetActivity(
            context,
            id,
            mainIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        // Small Icon (white silhouette for status bar)
        int smallIcon = context.Resources?.GetIdentifier("ic_notification", "drawable", context.PackageName) ?? 0;
        if (smallIcon == 0)
        {
            smallIcon = context.ApplicationInfo?.Icon != 0 
                ? context.ApplicationInfo!.Icon 
                : global::Android.Resource.Drawable.SymDefAppIcon;
        }

        // Large Icon (app logo decoded into full-color Bitmap)
        Bitmap? largeIcon = null;
        try
        {
            int appIconRes = context.Resources?.GetIdentifier("khadamat_icon", "mipmap", context.PackageName) ?? 0;
            if (appIconRes == 0)
            {
                appIconRes = context.ApplicationInfo?.Icon ?? 0;
            }
            if (appIconRes != 0 && context.Resources != null)
            {
                largeIcon = BitmapFactory.DecodeResource(context.Resources, appIconRes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Reminder] Error decoding large icon: {ex.Message}");
        }

        var defaultSound = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

        var builder = new NotificationCompat.Builder(context, AndroidReminderScheduler.ChannelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSubText("تذكير خدماوي")
            .SetStyle(new NotificationCompat.BigTextStyle()
                .SetBigContentTitle(title)
                .BigText(message)
                .SetSummaryText("تذكير مخصص"))
            .SetSmallIcon(smallIcon)
            .SetColor(global::Android.Graphics.Color.ParseColor("#6366f1"))
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetCategory(NotificationCompat.CategoryReminder)
            .SetSound(defaultSound)
            .SetVibrate(new long[] { 0, 300, 150, 300 })
            .SetLights(global::Android.Graphics.Color.ParseColor("#6366f1"), 1000, 500)
            .SetContentIntent(pendingIntent);

        if (largeIcon != null)
        {
            builder.SetLargeIcon(largeIcon);
        }

        notificationManager.Notify(id, builder.Build());
    }

    private void ScheduleNextOccurrence(Context context, Intent originalIntent, int id, ReminderRecurrenceType recurrence, long ticks, int intervalVal)
    {
        if (recurrence == ReminderRecurrenceType.Once) return;

        var alarmManager = (AlarmManager?)context.GetSystemService(global::Android.Content.Context.AlarmService);
        if (alarmManager == null) return;

        var scheduledTime = new TimeSpan(ticks > 0 ? ticks : new TimeSpan(20, 0, 0).Ticks);
        var nextTrigger = DateTime.Now;

        switch (recurrence)
        {
            case ReminderRecurrenceType.Daily:
                nextTrigger = nextTrigger.AddDays(intervalVal > 0 ? intervalVal : 1);
                break;
            case ReminderRecurrenceType.Weekly:
                nextTrigger = nextTrigger.AddDays(7);
                break;
            case ReminderRecurrenceType.IntervalHours:
                nextTrigger = nextTrigger.AddHours(intervalVal > 0 ? intervalVal : 24);
                break;
            case ReminderRecurrenceType.IntervalDays:
                nextTrigger = nextTrigger.AddDays(intervalVal > 0 ? intervalVal : 1);
                break;
            default:
                nextTrigger = nextTrigger.AddDays(1);
                break;
        }

        nextTrigger = new DateTime(nextTrigger.Year, nextTrigger.Month, nextTrigger.Day, scheduledTime.Hours, scheduledTime.Minutes, 0);

        var nextTriggerMillis = new DateTimeOffset(nextTrigger).ToUnixTimeMilliseconds();

        var repeatPendingIntent = PendingIntent.GetBroadcast(
            context,
            id,
            originalIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        if (repeatPendingIntent != null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, nextTriggerMillis, repeatPendingIntent);
            }
            else
            {
                alarmManager.Set(AlarmType.RtcWakeup, nextTriggerMillis, repeatPendingIntent);
            }
            Console.WriteLine($"[ReminderAlarmReceiver] Rescheduled next occurrence of ID {id} for {nextTrigger:yyyy-MM-dd HH:mm:ss}");
        }
    }
}
#endif
