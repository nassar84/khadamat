#if ANDROID
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Khadamat.Application.DTOs;
using Khadamat.MobileApp.Services;

namespace Khadamat.MobileApp.Platforms.Android;

public class AndroidReminderScheduler : IPlatformReminderScheduler
{
    public const string ChannelId = "khadamawy_reminders_channel";
    public const string ChannelName = "تذكيرات خدماوى";
    public const string ChannelDescription = "تذكيرات استخدام التطبيق والميزات الجديدة وتجربة النظام";

    public void Initialize()
    {
        CreateNotificationChannel();
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var context = global::Android.App.Application.Context;
            var notificationManager = (NotificationManager?)context.GetSystemService(global::Android.Content.Context.NotificationService);
            if (notificationManager == null) return;

            var existingChannel = notificationManager.GetNotificationChannel(ChannelId);
            if (existingChannel == null)
            {
                var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.Default)
                {
                    Description = ChannelDescription
                };
                channel.EnableLights(true);
                channel.EnableVibration(true);
                channel.SetShowBadge(true);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }

    public void ScheduleNotification(ActiveClientReminderDto reminder)
    {
        var context = global::Android.App.Application.Context;
        var alarmManager = (AlarmManager?)context.GetSystemService(global::Android.Content.Context.AlarmService);
        if (alarmManager == null) return;

        // Cancel previous alarm if any exists with this ID to prevent duplicate alarms
        CancelNotification(reminder.Id);

        // Calculate trigger time
        var now = DateTime.Now;
        var trigger = new DateTime(now.Year, now.Month, now.Day, reminder.ScheduledTime.Hours, reminder.ScheduledTime.Minutes, 0);
        if (trigger <= now)
        {
            // If already passed today, schedule for tomorrow
            trigger = trigger.AddDays(1);
        }

        // Check if trigger is past EndDate
        if (reminder.EndDate.HasValue && trigger > reminder.EndDate.Value.ToLocalTime())
        {
            Console.WriteLine($"[AndroidReminderScheduler] Reminder {reminder.Id} is past end date. Skipping.");
            return;
        }

        var triggerMillis = new DateTimeOffset(trigger).ToUnixTimeMilliseconds();

        var intent = new Intent(context, Java.Lang.Class.FromType(typeof(ReminderAlarmReceiver)));
        intent.PutExtra("reminder_id", reminder.Id);
        intent.PutExtra("title", reminder.Title);
        intent.PutExtra("message", reminder.Message);
        intent.PutExtra("action_route", reminder.ActionRoute);
        intent.PutExtra("recurrence_type", (int)reminder.RecurrenceType);
        intent.PutExtra("interval_value", reminder.IntervalValue);
        intent.PutExtra("scheduled_time_ticks", reminder.ScheduledTime.Ticks);

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            reminder.Id,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        if (pendingIntent == null) return;

        // Inexact alarm respecting battery optimization and Google Play policies
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerMillis, pendingIntent);
        }
        else
        {
            alarmManager.Set(AlarmType.RtcWakeup, triggerMillis, pendingIntent);
        }

        Console.WriteLine($"[AndroidReminderScheduler] Scheduled reminder '{reminder.Title}' (ID: {reminder.Id}) for {trigger:yyyy-MM-dd HH:mm:ss}");
    }

    public void CancelNotification(int reminderId)
    {
        var context = global::Android.App.Application.Context;
        var alarmManager = (AlarmManager?)context.GetSystemService(global::Android.Content.Context.AlarmService);

        var intent = new Intent(context, Java.Lang.Class.FromType(typeof(ReminderAlarmReceiver)));
        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            reminderId,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        if (pendingIntent != null && alarmManager != null)
        {
            alarmManager.Cancel(pendingIntent);
            pendingIntent.Cancel();
        }

        var notificationManager = (NotificationManager?)context.GetSystemService(global::Android.Content.Context.NotificationService);
        notificationManager?.Cancel(reminderId);

        Console.WriteLine($"[AndroidReminderScheduler] Cancelled reminder ID {reminderId}");
    }

    public void CancelAllNotifications()
    {
        var context = global::Android.App.Application.Context;
        var notificationManager = (NotificationManager?)context.GetSystemService(global::Android.Content.Context.NotificationService);
        notificationManager?.CancelAll();

        // Also cancel alarms for cached reminders
        var cached = LocalReminderScheduler.GetCachedReminders();
        foreach (var rem in cached)
        {
            CancelNotification(rem.Id);
        }

        Console.WriteLine("[AndroidReminderScheduler] Cancelled all reminders");
    }

    public Task<bool> RequestPermissionAsync()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+ (API 33+)
        {
            var context = global::Android.App.Application.Context;
            var permissionStatus = ContextCompat.CheckSelfPermission(context, "android.permission.POST_NOTIFICATIONS");
            if (permissionStatus == global::Android.Content.PM.Permission.Granted)
            {
                return Task.FromResult(true);
            }

            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity != null)
            {
                ActivityCompat.RequestPermissions(activity, new[] { "android.permission.POST_NOTIFICATIONS" }, 101);
            }
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
#endif
