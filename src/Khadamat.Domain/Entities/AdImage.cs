using System;

namespace Khadamat.Domain.Entities;

public class AdImage : BaseEntity
{
    public int AdId { get; private set; }
    public string ImagePath { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    
    // Navigation Property
    public virtual Ad Ad { get; private set; } = null!;
    
    // Constructor for EF Core
    protected AdImage() { }

    public AdImage(int adId, string imagePath, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException("Image path is required.");

        AdId = adId;
        ImagePath = imagePath;
        DisplayOrder = displayOrder;
    }

    public void UpdateImagePath(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException("Image path is required.");

        ImagePath = imagePath;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
    }
}
