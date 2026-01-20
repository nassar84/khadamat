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

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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
            .OrderBy(c => c.Order)
            .Select(c => new MainCategoryDto 
            { 
                Id = c.Id, 
                Name = c.Name, 
                Icon = c.Icon, 
                Color = c.Color, 
                Order = c.Order 
            })
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<MainCategoryDto>>.Succeed(categories));
    }

    [HttpGet("main/{mainCategoryId}/categories")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories(int mainCategoryId)
    {
        var categories = await _context.Categories
            .Where(c => c.MainCategoryId == mainCategoryId)
            .Include(c => c.MainCategory)
            .Select(c => new CategoryDto 
            { 
                Id = c.Id, 
                Name = c.Name,
                MainCategoryId = c.MainCategoryId,
                MainCategoryName = c.MainCategory.Name
            })
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Succeed(categories));
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
                MainCategoryName = c.MainCategory.Name
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
                MainCategoryName = s.Category.MainCategory.Name
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
            .Include(s => s.Category)
                .ThenInclude(c => c.MainCategory)
            .Select(s => new SubCategoryDto 
            { 
                Id = s.Id, 
                Name = s.Name,
                CategoryId = s.CategoryId,
                CategoryName = s.Category.Name,
                MainCategoryId = s.Category.MainCategoryId,
                MainCategoryName = s.Category.MainCategory.Name
            })
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<SubCategoryDto>>.Succeed(subCategories));
    }
    // --- Main Categories ---
    [HttpPost("main")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateMainCategory(MainCategoryDto dto)
    {
        var category = new MainCategory(dto.Name, dto.Icon, dto.Color, dto.Order);
        _context.MainCategories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<int>.Succeed(category.Id));
    }

    [HttpPut("main/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMainCategory(int id, MainCategoryDto dto)
    {
        var category = await _context.MainCategories.FindAsync(id);
        if (category == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        category.Update(dto.Name, dto.Icon, dto.Color, dto.Order);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("main/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMainCategory(int id)
    {
        var category = await _context.MainCategories.FindAsync(id);
        if (category == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        _context.MainCategories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // --- Categories ---
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateCategory(CategoryDto dto)
    {
        var category = new Category(dto.Name, dto.MainCategoryId);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<int>.Succeed(category.Id));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateCategory(int id, CategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        category.Update(dto.Name, dto.MainCategoryId);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // --- SubCategories ---
    [HttpPost("sub")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateSubCategory(SubCategoryDto dto)
    {
        var sub = new SubCategory(dto.Name, dto.CategoryId);
        _context.SubCategories.Add(sub);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<int>.Succeed(sub.Id));
    }

    [HttpPut("sub/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSubCategory(int id, SubCategoryDto dto)
    {
        var sub = await _context.SubCategories.FindAsync(id);
        if (sub == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        sub.Update(dto.Name, dto.CategoryId);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("sub/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSubCategory(int id)
    {
        var sub = await _context.SubCategories.FindAsync(id);
        if (sub == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        _context.SubCategories.Remove(sub);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }
}
