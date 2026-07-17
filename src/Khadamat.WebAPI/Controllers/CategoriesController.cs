using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Khadamat.Application.Common.Models;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Services;
using System.IO;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public CategoriesController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpGet("main")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MainCategoryDto>>>> GetMainCategories()
    {
        var categories = await _context.MainCategories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new MainCategoryDto 
            { 
                Id = c.Id, 
                Name = c.Name, 
                Icon = c.Icon, 
                ImageUrl = c.ImageUrl,
                Color = c.Color, 
                DisplayOrder = c.DisplayOrder 
            })
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<MainCategoryDto>>.Succeed(categories));
    }

    [HttpGet("main/{mainCategoryId}/categories")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories(int mainCategoryId)
    {
        try {
            var categories = await _context.Categories
                .Where(c => c.MainCategoryId == mainCategoryId)
                .OrderBy(c => c.DisplayOrder)
                .Include(c => c.MainCategory)
                .Select(c => new CategoryDto 
                { 
                    Id = c.Id, 
                    Name = c.Name,
                    MainCategoryId = c.MainCategoryId,
                    MainCategoryName = c.MainCategory.Name,
                    ImageUrl = c.ImageUrl,
                    DisplayOrder = c.DisplayOrder
                })
                .ToListAsync();
            
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.Succeed(categories));
        } catch (Exception ex) {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpGet("categories/{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.MainCategory)
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto 
            { 
                Id = c.Id, 
                Name = c.Name,
                MainCategoryId = c.MainCategoryId,
                MainCategoryName = c.MainCategory.Name,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder
            })
            .FirstOrDefaultAsync();
        
        if (category == null) return NotFound(ApiResponse<CategoryDto>.Fail("Category not found"));
        return Ok(ApiResponse<CategoryDto>.Succeed(category));
    }

    [HttpGet("subcategories/{id}")]
    public async Task<ActionResult<ApiResponse<SubCategoryDto>>> GetSubCategory(int id)
    {
        var subCategory = await _context.SubCategories
            .Include(s => s.Category)
                .ThenInclude(c => c.MainCategory)
            .Where(s => s.Id == id)
            .Select(s => new SubCategoryDto 
            { 
                Id = s.Id, 
                Name = s.Name,
                CategoryId = s.CategoryId,
                CategoryName = s.Category.Name,
                MainCategoryId = s.Category.MainCategoryId,
                MainCategoryName = s.Category.MainCategory.Name,
                ImageUrl = s.ImageUrl,
                DisplayOrder = s.DisplayOrder
            })
            .FirstOrDefaultAsync();
        
        if (subCategory == null) return NotFound(ApiResponse<SubCategoryDto>.Fail("SubCategory not found"));
        return Ok(ApiResponse<SubCategoryDto>.Succeed(subCategory));
    }

    [HttpGet("{categoryId}/subcategories")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubCategoryDto>>>> GetSubCategories(int categoryId)
    {
        var subCategories = await _context.SubCategories
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.DisplayOrder)
            .Include(s => s.Category)
                .ThenInclude(c => c.MainCategory)
            .Select(s => new SubCategoryDto 
            { 
                Id = s.Id, 
                Name = s.Name,
                CategoryId = s.CategoryId,
                CategoryName = s.Category.Name,
                MainCategoryId = s.Category.MainCategoryId,
                MainCategoryName = s.Category.MainCategory.Name,
                ImageUrl = s.ImageUrl,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<SubCategoryDto>>.Succeed(subCategories));
    }

    // --- Main Categories ---
    [HttpPost("main")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateMainCategory(MainCategoryDto dto)
    {
        var category = new MainCategory(dto.Name, dto.Icon, dto.Color, dto.DisplayOrder)
        {
            ImageUrl = ImageNamingHelper.ExtractFileName(dto.ImageUrl)
        };
        _context.MainCategories.Add(category);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(category.ImageUrl))
        {
            var finalName = ImageNamingHelper.RenameImage(category.ImageUrl, "maincategories", $"cat_{category.Id}");
            if (finalName != category.ImageUrl)
            {
                category.ImageUrl = finalName;
                await _context.SaveChangesAsync();
            }
        }
        return Ok(ApiResponse<int>.Succeed(category.Id));
    }

    [HttpPut("main/{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMainCategory(int id, MainCategoryDto dto)
    {
        var category = await _context.MainCategories.FindAsync(id);
        if (category == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        var cleanDtoImage = ImageNamingHelper.ExtractFileName(dto.ImageUrl);

        if (!string.IsNullOrEmpty(cleanDtoImage) && cleanDtoImage != category.ImageUrl)
        {
            // Delete old file
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "maincategories", category.ImageUrl);
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }
            cleanDtoImage = ImageNamingHelper.RenameImage(cleanDtoImage, "maincategories", $"cat_{id}");
        }
        else if (string.IsNullOrEmpty(cleanDtoImage) && !string.IsNullOrEmpty(category.ImageUrl))
        {
            // Delete old file
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "maincategories", category.ImageUrl);
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }
        }

        category.Update(dto.Name, dto.Icon, dto.Color, dto.DisplayOrder, cleanDtoImage);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("main/{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMainCategory(int id)
    {
        var category = await _context.MainCategories.FindAsync(id);
        if (category == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        // Delete image file if exists
        if (!string.IsNullOrEmpty(category.ImageUrl))
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "maincategories", category.ImageUrl);
            if (System.IO.File.Exists(path))
            {
                try { System.IO.File.Delete(path); } catch { }
            }
        }

        _context.MainCategories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // --- Categories ---
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateCategory(CategoryDto dto)
    {
        var category = new Category(dto.Name, dto.MainCategoryId, ImageNamingHelper.ExtractFileName(dto.ImageUrl), dto.DisplayOrder);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(category.ImageUrl))
        {
            var finalName = ImageNamingHelper.RenameImage(category.ImageUrl, "categories", $"c_{dto.MainCategoryId}_{category.Id}");
            if (finalName != category.ImageUrl)
            {
                category.ImageUrl = finalName;
                await _context.SaveChangesAsync();
            }
        }
        return Ok(ApiResponse<int>.Succeed(category.Id));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateCategory(int id, CategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        var cleanDtoImage = ImageNamingHelper.ExtractFileName(dto.ImageUrl);

        if (!string.IsNullOrEmpty(cleanDtoImage) && cleanDtoImage != category.ImageUrl)
        {
            // Delete old file
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories", category.ImageUrl);
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }
            cleanDtoImage = ImageNamingHelper.RenameImage(cleanDtoImage, "categories", $"c_{dto.MainCategoryId}_{id}");
        }
        else if (string.IsNullOrEmpty(cleanDtoImage) && !string.IsNullOrEmpty(category.ImageUrl))
        {
            // Delete old file
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories", category.ImageUrl);
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }
        }

        category.Update(dto.Name, dto.MainCategoryId, cleanDtoImage, dto.DisplayOrder);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        // Delete image file if exists
        if (!string.IsNullOrEmpty(category.ImageUrl))
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories", category.ImageUrl);
            if (System.IO.File.Exists(path))
            {
                try { System.IO.File.Delete(path); } catch { }
            }
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // --- SubCategories ---
    [HttpPost("sub")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateSubCategory(SubCategoryDto dto)
    {
        var sub = new SubCategory(dto.Name, dto.CategoryId, ImageNamingHelper.ExtractFileName(dto.ImageUrl), dto.DisplayOrder);
        _context.SubCategories.Add(sub);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(sub.ImageUrl))
        {
            var finalName = ImageNamingHelper.RenameImage(sub.ImageUrl, "subcategories", $"subc_{dto.CategoryId}_{sub.Id}");
            if (finalName != sub.ImageUrl)
            {
                sub.ImageUrl = finalName;
                await _context.SaveChangesAsync();
            }
        }
        return Ok(ApiResponse<int>.Succeed(sub.Id));
    }

    [HttpPut("sub/{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSubCategory(int id, SubCategoryDto dto)
    {
        var sub = await _context.SubCategories.FindAsync(id);
        if (sub == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        var cleanDtoImage = ImageNamingHelper.ExtractFileName(dto.ImageUrl);

        if (!string.IsNullOrEmpty(cleanDtoImage) && cleanDtoImage != sub.ImageUrl)
        {
            // Delete old file
            if (!string.IsNullOrEmpty(sub.ImageUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "subcategories", sub.ImageUrl);
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }
            cleanDtoImage = ImageNamingHelper.RenameImage(cleanDtoImage, "subcategories", $"subc_{dto.CategoryId}_{id}");
        }
        else if (string.IsNullOrEmpty(cleanDtoImage) && !string.IsNullOrEmpty(sub.ImageUrl))
        {
            // Delete old file
            if (!string.IsNullOrEmpty(sub.ImageUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "subcategories", sub.ImageUrl);
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }
        }

        sub.Update(dto.Name, dto.CategoryId, cleanDtoImage, dto.DisplayOrder);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("sub/{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSubCategory(int id)
    {
        var sub = await _context.SubCategories.FindAsync(id);
        if (sub == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        // Delete image file if exists
        if (!string.IsNullOrEmpty(sub.ImageUrl))
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "subcategories", sub.ImageUrl);
            if (System.IO.File.Exists(path))
            {
                try { System.IO.File.Delete(path); } catch { }
            }
        }

        _context.SubCategories.Remove(sub);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }
}
