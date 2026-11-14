using System;
using Khadamat.Domain.Common;

namespace Khadamat.Domain.Entities;

/// <summary>
/// Professional Titles Dictionary rule for smart, respectful Egyptian addressing.
/// Ensures doctors -> دكتور/دكتورة, engineers -> باشمهندس/باشمهندسة,
/// and strictly teachers -> مستر / مس.
/// </summary>
public class ProfessionalTitleRule : BaseEntity
{
    /// <summary>
    /// Profession or keyword to match (e.g. "مدرس", "طبيب", "مهندس", "محام", "صيدلي", "فني", "محاسب")
    /// </summary>
    public string ProfessionKeyword { get; set; } = string.Empty;

    /// <summary>
    /// Male greeting title e.g. "مستر", "دكتور", "باشمهندس", "أستاذ"
    /// </summary>
    public string MaleTitle { get; set; } = string.Empty;

    /// <summary>
    /// Female greeting title e.g. "مس", "دكتورة", "باشمهندسة", "أستاذة"
    /// </summary>
    public string FemaleTitle { get; set; } = string.Empty;

    /// <summary>
    /// Default / neutral fallback title e.g. "أستاذ / أستاذة"
    /// </summary>
    public string DefaultTitle { get; set; } = "أستاذ / أستاذة";

    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
    public string? Notes { get; set; }
}
