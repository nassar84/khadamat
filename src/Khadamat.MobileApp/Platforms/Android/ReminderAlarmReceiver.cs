#if ANDROID
using System;
using Android.App;
using Android.Content;
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

        // Intent to launch MainActivity with deep link route
        var mainIntent = new Intent(context, Java.Lang.Class.FromType(typeof(MainActivity)));
        mainIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        mainIntent.PutExtra("action_route", actionRoute);
        mainIntent.PutExtra("from_reminder", true);
        mainIntent.PutExtra("reminder_id", id);

        var pendingIntent = PendingIntent.GetActivity(
            context,
            id,
            mainIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var icon = context.ApplicationInfo?.Icon != 0 
            ? context.ApplicationInfo!.Icon 
            : global::Android.Resource.Drawable.SymDefAppIcon;

        var builder = new NotificationCompat.Builder(context, AndroidReminderScheduler.ChannelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(message))
            .SetSmallIcon(icon)
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityDefault)
            .SetContentIntent(pendingIntent);

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
