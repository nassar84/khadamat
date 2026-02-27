using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class MarketplaceSubCategory : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual MarketplaceCategory Category { get; set; } = null!;
    public virtual ICollection<MarketplaceItem> Items { get; set; } = new List<MarketplaceItem>();

    public MarketplaceSubCategory() { }
    public MarketplaceSubCategory(string name, int categoryId, int displayOrder)
    {
        Name = name;
        CategoryId = categoryId;
        DisplayOrder = displayOrder;
    }

    public void Update(string name, int categoryId, int displayOrder)
    {
        Name = name;
        CategoryId = categoryId;
        DisplayOrder = displayOrder;
        UpdatedAt = System.DateTime.UtcNow;
    }
}
