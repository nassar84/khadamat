using System.Collections.Generic;

namespace Khadamat.Application.DTOs;

public class MainCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MainCategoryId { get; set; }
    public string? MainCategoryName { get; set; }
}

public class SubCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int MainCategoryId { get; set; }
    public string? MainCategoryName { get; set; }
}
