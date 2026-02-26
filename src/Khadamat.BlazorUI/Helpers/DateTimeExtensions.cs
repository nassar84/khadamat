using System;

namespace Khadamat.BlazorUI.Helpers;

public static class DateTimeExtensions
{
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan <= TimeSpan.FromSeconds(60))
            return "الآن";

        if (timeSpan <= TimeSpan.FromMinutes(60))
            return $"منذ {timeSpan.Minutes} دقيقة";

        if (timeSpan <= TimeSpan.FromHours(24))
            return $"منذ {timeSpan.Hours} ساعة";

        if (timeSpan <= TimeSpan.FromDays(30))
            return $"منذ {timeSpan.Days} يوم";

        if (timeSpan <= TimeSpan.FromDays(365))
            return $"منذ {timeSpan.Days / 30} شهر";

        return $"منذ {timeSpan.Days / 365} سنة";
    }
}
