using System;

namespace Khadamat.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty; // Store as string to support different ID types
    public string Details { get; private set; } = string.Empty;

    protected AuditLog() { }

    public AuditLog(string userId, string action, string entityName, string entityId, string details)
    {
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        Details = details;
    }
}
