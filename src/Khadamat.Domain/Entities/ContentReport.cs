namespace Khadamat.Domain.Entities;

public enum ReportTargetType { Review = 1, Comment = 2 }
public enum ReportStatus     { Pending = 0, Resolved = 1, Dismissed = 2 }

public class ContentReport : BaseEntity
{
    public ReportTargetType TargetType  { get; set; }
    public int              TargetId    { get; set; }   // Rating.Id or Comment.Id
    public string           ReporterId  { get; set; } = string.Empty;
    public string           Reason      { get; set; } = string.Empty;
    public ReportStatus     Status      { get; set; } = ReportStatus.Pending;
    public string?          AdminNote   { get; set; }
    public string?          AdminId     { get; set; }
    public DateTime?        ResolvedAt  { get; set; }

    protected ContentReport() { }

    public ContentReport(ReportTargetType targetType, int targetId, string reporterId, string reason)
    {
        TargetType = targetType;
        TargetId   = targetId;
        ReporterId = reporterId;
        Reason     = reason;
        Status     = ReportStatus.Pending;
    }

    public void Resolve(string adminId, string? note = null)
    {
        Status     = ReportStatus.Resolved;
        AdminId    = adminId;
        AdminNote  = note;
        ResolvedAt = DateTime.UtcNow;
    }

    public void Dismiss(string adminId, string? note = null)
    {
        Status     = ReportStatus.Dismissed;
        AdminId    = adminId;
        AdminNote  = note;
        ResolvedAt = DateTime.UtcNow;
    }
}
