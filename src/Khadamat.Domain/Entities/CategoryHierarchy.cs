using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class MainCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Order { get; set; }
    
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public MainCategory() { }
    public MainCategory(string name, string icon, string color, int order)
    {
        Name = name;
        Icon = icon;
        Color = color;
        Order = order;
    }

    public void Update(string name, string icon, string color, int order, string? imageUrl = null)
    {
        Name = name;
        Icon = icon;
        Color = color;
        Order = order;
        ImageUrl = imageUrl;
        UpdatedAt = System.DateTime.UtcNow;
    }
}

public class Category : BaseEntity
{
    public int MainCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public virtual MainCategory MainCategory { get; set; } = null!;
    public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public Category() { }
    public Category(string name, int mainCategoryId)
    {
        Name = name;
        MainCategoryId = mainCategoryId;
    }

    public void Update(string name, int mainCategoryId)
    {
        Name = name;
        MainCategoryId = mainCategoryId;
        UpdatedAt = System.DateTime.UtcNow;
    }
}

public class SubCategory : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public SubCategory() { }
    public SubCategory(string name, int categoryId)
    {
        Name = name;
        CategoryId = categoryId;
    }

    public void Update(string name, int categoryId)
    {
        Name = name;
        CategoryId = categoryId;
        UpdatedAt = System.DateTime.UtcNow;
    }
}
