using System.ComponentModel.DataAnnotations;

namespace Khadamat.Application.DTOs;

public class SpecializationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? SubCategoryId { get; set; }
    public string? SubCategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int ServicesCount { get; set; }
}

public class CreateSpecializationDto
{
    [Required(ErrorMessage = "اسم التخصص مطلوب")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int DisplayOrder { get; set; } = 0;
}

public class UpdateSpecializationDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "اسم التخصص مطلوب")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
