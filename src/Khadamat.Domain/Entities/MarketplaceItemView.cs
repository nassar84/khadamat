using System;

namespace Khadamat.Domain.Entities;

public class MarketplaceItemView
{
    public int Id { get; set; }
    public int MarketplaceItemId { get; set; }
    public virtual MarketplaceItem MarketplaceItem { get; set; } = null!;
    public string UserId { get; set; } = string.Empty; // Identity User Id
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
