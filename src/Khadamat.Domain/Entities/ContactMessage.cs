namespace Khadamat.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// technical | account | provider | complaint | suggestion | other
    /// </summary>
    public string SubjectType { get; set; } = "other";

    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByUserId { get; set; }
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Optional: if the message was sent by a logged-in user
    /// </summary>
    public string? UserId { get; set; }
}
