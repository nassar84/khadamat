using System;
using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class Specialization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // ربط اختياري بالتصنيف الرئيسي أو الفرعي (مثل: تصنيف المدرسين أو المرحلة الابتدائية)
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual Category? Category { get; set; }
    public virtual SubCategory? SubCategory { get; set; }
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public Specialization() { }

    public Specialization(string name, int? categoryId = null, int? subCategoryId = null, int displayOrder = 0)
    {
        Name = name;
        CategoryId = categoryId;
        SubCategoryId = subCategoryId;
        DisplayOrder = displayOrder;
        IsActive = true;
    }
}
