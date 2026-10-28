using System;

namespace Khadamat.Domain.Entities;

public class ReminderBankItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // Morning, Midday, Evening, Friday, Promo, General, etc.
    public int OrderIndex { get; set; } = 1;
    public TimeSpan? SuggestedTime { get; set; }
    public string? ImageUrl { get; set; }
    public string ActionRoute { get; set; } = "/";
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsedAt { get; set; }
}
