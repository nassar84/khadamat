using System;
using Khadamat.Domain.Enums;

namespace Khadamat.Application.DTOs;

public class ServiceRequestDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public string ServiceIcon { get; set; } = "fa-briefcase";
    
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    
    public RequestStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ProviderNotes { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? PreferredDate { get; set; }
}

public class CreateServiceRequestDto
{
    public int ServiceId { get; set; }
    public string? Notes { get; set; }
    public DateTime? PreferredDate { get; set; }
}
