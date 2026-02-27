using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class MarketplaceCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<MarketplaceSubCategory> SubCategories { get; set; } = new List<MarketplaceSubCategory>();
    public virtual ICollection<MarketplaceItem> Items { get; set; } = new List<MarketplaceItem>();

    public MarketplaceCategory() { }
    public MarketplaceCategory(string name, int displayOrder, string? icon = null, string? imageUrl = null)
    {
        Name = name;
        DisplayOrder = displayOrder;
        Icon = icon;
        ImageUrl = imageUrl;
    }

    public void Update(string name, int displayOrder, string? icon = null, string? imageUrl = null)
    {
        Name = name;
        DisplayOrder = displayOrder;
        Icon = icon;
        ImageUrl = imageUrl;
        UpdatedAt = System.DateTime.UtcNow;
    }
}
