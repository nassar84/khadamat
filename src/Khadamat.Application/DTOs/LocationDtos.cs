using System;

namespace Khadamat.Application.DTOs;

public class GovernorateDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool Approved { get; set; }
    public string? Notes { get; set; }
    public string? UserCreated { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CityDto
{
    public int Id { get; set; }
    public int GovernorateId { get; set; }
    public string? GovernorateName { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool Approved { get; set; }
    public string? Notes { get; set; }
    public string? UserCreated { get; set; }
    public DateTime CreatedAt { get; set; }
}
