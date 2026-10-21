using System;

namespace Khadamat.Application.Common.Helpers;

public static class ClinicServiceHelper
{
    private static readonly string[] DoctorPrefixes = new[]
    {
        "د/",
        "د /",
        "د.",
        "د .",
        "دكتور",
        "دكتورة",
        "دكتوره",
        "أ.د",
        "ا.د",
        "أ. د",
        "ا. د"
    };

    private static readonly string[] ExcludedPrefixes = new[]
    {
        "مركز",
        "مستشفى",
        "مستشفي",
        "مجمع",
        "عيادة",
        "عيادات",
        "معمل",
        "مختبر",
        "صيدلية",
        "خدمة"
    };

    public static bool HasDoctorTitle(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var trimmed = name.Trim();
        foreach (var prefix in DoctorPrefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0)
        {
            var first = words[0];
            if (first.Equals("دكتور", StringComparison.OrdinalIgnoreCase) ||
                first.Equals("دكتورة", StringComparison.OrdinalIgnoreCase) ||
                first.Equals("دكتوره", StringComparison.OrdinalIgnoreCase) ||
                first.StartsWith("د/", StringComparison.OrdinalIgnoreCase) ||
                first.StartsWith("د.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsExcludedEntity(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var trimmed = name.Trim();
        foreach (var prefix in ExcludedPrefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static string FormatDoctorNameIfNeeded(string name, bool isClinic)
    {
        if (!isClinic || string.IsNullOrWhiteSpace(name))
            return name;

        var trimmed = name.Trim();
        if (HasDoctorTitle(trimmed) || IsExcludedEntity(trimmed))
            return trimmed;

        return $"د/ {trimmed}";
    }
}
