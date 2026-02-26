using System;

namespace Khadamat.Domain.Entities;

public class MarketplaceImage : BaseEntity
{
    public int MarketplaceItemId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }

    public virtual MarketplaceItem MarketplaceItem { get; private set; } = null!;

    protected MarketplaceImage() { }

    public MarketplaceImage(int marketplaceItemId, string imageUrl, int displayOrder = 0, bool isMain = false)
    {
        MarketplaceItemId = marketplaceItemId;
        ImageUrl = imageUrl;
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
}
