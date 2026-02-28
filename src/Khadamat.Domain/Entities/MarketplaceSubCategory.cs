using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class MarketplaceSubCategory : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual MarketplaceCategory Category { get; set; } = null!;
    public virtual ICollection<MarketplaceItem> Items { get; set; } = new List<MarketplaceItem>();

    public MarketplaceSubCategory() { }
    public MarketplaceSubCategory(string name, int categoryId, int displayOrder, string? imageUrl = null)
    {
        Name = name;
        CategoryId = categoryId;
        DisplayOrder = displayOrder;
        ImageUrl = imageUrl;
    }

    public void Update(string name, int categoryId, int displayOrder, string? imageUrl = null)
    {
        Name = name;
        CategoryId = categoryId;
        DisplayOrder = displayOrder;
        ImageUrl = imageUrl;
        UpdatedAt = System.DateTime.UtcNow;
    }
}
