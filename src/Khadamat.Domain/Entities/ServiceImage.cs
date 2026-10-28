using System;

namespace Khadamat.Domain.Entities;

public class ServiceImage : BaseEntity
{
    public int ServiceId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }

    // Navigation Property
    public virtual Service Service { get; private set; } = null!;

    // Constructor for EF Core
    protected ServiceImage() { }

    public ServiceImage(int serviceId, string imageUrl, int displayOrder = 0, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required.", nameof(imageUrl));

        ServiceId = serviceId;
        ImageUrl = imageUrl.Trim();
        DisplayOrder = displayOrder;
        IsMain = isMain;
    }

    public void SetAsMain(bool isMain)
    {
        IsMain = isMain;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOrder(int order)
    {
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required.", nameof(imageUrl));

        ImageUrl = imageUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
