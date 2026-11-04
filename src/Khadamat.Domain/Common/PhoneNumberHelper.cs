using System;
using System.Linq;
using System.Text;

namespace Khadamat.Domain.Common;

/// <summary>
/// توحيد وتحويل أرقام الهواتف والواتساب والتعامل مع الأرقام العربية والإنجليزية
/// </summary>
public static class PhoneNumberHelper
{
    /// <summary>
    /// يحول جميع الأرقام العربية والمشرقية والفارسية إلى أرقام إنجليزية (0-9)
    /// </summary>
    public static string ToEnglishDigits(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? string.Empty;

        var sb = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (c >= '\u0660' && c <= '\u0669') // الأرقام العربية المشرقية ٠-٩
            {
                sb.Append((char)('0' + (c - '\u0660')));
            }
            else if (c >= '\u06F0' && c <= '\u06F9') // الأرقام الفارسية/الأوردية ۰-۹
            {
                sb.Append((char)('0' + (c - '\u06F0')));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// تنظيف وتوحيد رقم الهاتف لتخزينه في قاعدة البيانات بالإنجليزية بدون مسافات أو رموز زائدة.
    /// يحافظ على علامة + في البداية إن وجدت.
    /// </summary>
    public static string? NormalizeDigits(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;

        var converted = ToEnglishDigits(phone).Trim();
        var sb = new StringBuilder(converted.Length);

        bool hasPlus = converted.StartsWith("+");
        if (hasPlus) sb.Append('+');

        foreach (char c in converted)
        {
            if (c >= '0' && c <= '9')
            {
                sb.Append(c);
            }
        }

        var result = sb.ToString();
        return string.IsNullOrEmpty(result) || result == "+" ? null : result;
    }

    /// <summary>
    /// تجهيز رقم الهاتف لإجراء مكالمة هاتفية عبر tel:
    /// </summary>
    public static string NormalizeForCall(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

        var normalized = NormalizeDigits(phone);
        return normalized ?? string.Empty;
    }

    /// <summary>
    /// رابط tel: جاهز للنقر
    /// </summary>
    public static string GetTelUrl(string? phone)
    {
        var clean = NormalizeForCall(phone);
        if (string.IsNullOrEmpty(clean)) return "#";
        return $"tel:{clean}";
    }

    /// <summary>
    /// تجهيز رقم الواتساب لروابط wa.me (أرقام إنجليزية فقط مع كود الدولة وبدون + أو أصفار دولية)
    /// </summary>
    public static string NormalizeForWhatsApp(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

        var converted = ToEnglishDigits(phone);
        var sb = new StringBuilder(converted.Length);

        // أرقام فقط
        foreach (char c in converted)
        {
            if (c >= '0' && c <= '9')
            {
                sb.Append(c);
            }
        }

        var digits = sb.ToString();
        if (string.IsNullOrEmpty(digits)) return string.Empty;

        // إزالة الأصفار الدولية 00 في البداية إن وجدت
        if (digits.StartsWith("00"))
        {
            digits = digits.Substring(2);
        }

        // معالجة الأرقام المصرية:
        // رقم موبايل مصري يبدأ بـ 01 وطوله 11 رقم (010, 011, 012, 015)
        if (digits.StartsWith("01") && digits.Length == 11)
        {
            digits = "2" + digits; // يصبح 2010xxxxxxxx
        }
        // رقم موبايل مصري ناقص الصفر في البداية (10 أرقام تبدأ بـ 1)
        else if (digits.Length == 10 && (digits.StartsWith("10") || digits.StartsWith("11") || digits.StartsWith("12") || digits.StartsWith("15")))
        {
            digits = "20" + digits;
        }
        // رقم مصري يبدأ بـ 0 وطوله 10 أو 11 (أرضي أو موبايل)
        else if (digits.StartsWith("0") && digits.Length >= 9 && digits.Length <= 11)
        {
            digits = "2" + digits.TrimStart('0');
        }

        return digits;
    }

    /// <summary>
    /// رابط محادثة واتساب كامل وموثوق wa.me
    /// </summary>
    public static string GetWhatsAppUrl(string? phone, string? message = null)
    {
        var clean = NormalizeForWhatsApp(phone);
        if (string.IsNullOrEmpty(clean)) return "#";

        if (string.IsNullOrWhiteSpace(message))
        {
            return $"https://wa.me/{clean}";
        }

        return $"https://wa.me/{clean}?text={Uri.EscapeDataString(message)}";
    }
}
