using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class SpecializationsController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public SpecializationsController(KhadamatDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// جلب قائمة التخصصات مع إمكانية الفلترة حسب التصنيف
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSpecializations([FromQuery] int? categoryId, [FromQuery] int? subCategoryId, [FromQuery] string? search)
    {
        var query = _context.Specializations
            .Include(s => s.Category)
            .Include(s => s.SubCategory)
            .Include(s => s.Services)
            .Where(s => s.IsActive && !s.IsDeleted);

        if (subCategoryId.HasValue)
        {
            // عند تحديد subcategory: التخصصات المرتبطة بهذا الـ subcategory + العامة (بدون تصنيف) + المرتبطة بـ category فقط
            query = query.Where(s =>
                s.SubCategoryId == subCategoryId.Value ||
                s.CategoryId == null ||
                (s.SubCategoryId == null && s.CategoryId != null));
        }
        else if (categoryId.HasValue)
        {
            // عند تحديد category فقط: التخصصات المرتبطة بهذه الـ category أو العامة
            query = query.Where(s =>
                s.CategoryId == categoryId.Value ||
                s.CategoryId == null);
        }
        // بدون فلتر: إرجاع كل التخصصات النشطة

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search) || (s.Description != null && s.Description.ToLower().Contains(search)));
        }

        var list = await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Select(s => new SpecializationDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                CategoryId = s.CategoryId,
                CategoryName = s.Category != null ? s.Category.Name : null,
                SubCategoryId = s.SubCategoryId,
                SubCategoryName = s.SubCategory != null ? s.SubCategory.Name : null,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                ServicesCount = s.Services.Count(svc => !svc.IsDeleted)
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SpecializationDto>>.Succeed(list));
    }

    /// <summary>
    /// جلب تخصص بواسطة المعرف
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _context.Specializations
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Services)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (s == null) return NotFound(ApiResponse<SpecializationDto>.Fail("التخصص غير موجود"));

        var dto = new SpecializationDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            CategoryId = s.CategoryId,
            CategoryName = s.Category?.Name,
            SubCategoryId = s.SubCategoryId,
            SubCategoryName = s.SubCategory?.Name,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            ServicesCount = s.Services.Count(svc => !svc.IsDeleted)
        };

        return Ok(ApiResponse<SpecializationDto>.Succeed(dto));
    }

    /// <summary>
    /// إضافة تخصص جديد
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSpecializationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(ApiResponse<SpecializationDto>.Fail("اسم التخصص مطلوب"));

        var trimmedName = dto.Name.Trim();

        // فحص التكرار لنفس التصنيف
        var exists = await _context.Specializations
            .AnyAsync(s => s.Name.ToLower() == trimmedName.ToLower() && s.SubCategoryId == dto.SubCategoryId && s.CategoryId == dto.CategoryId);

        if (exists)
        {
            var existing = await _context.Specializations
                .FirstAsync(s => s.Name.ToLower() == trimmedName.ToLower() && s.SubCategoryId == dto.SubCategoryId && s.CategoryId == dto.CategoryId);

            return Ok(ApiResponse<SpecializationDto>.Succeed(new SpecializationDto
            {
                Id = existing.Id,
                Name = existing.Name,
                CategoryId = existing.CategoryId,
                SubCategoryId = existing.SubCategoryId
            }, "التخصص مسجل بالفعل"));
        }

        var spec = new Specialization(trimmedName, dto.CategoryId, dto.SubCategoryId, dto.DisplayOrder)
        {
            Description = dto.Description
        };

        _context.Specializations.Add(spec);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<SpecializationDto>.Succeed(new SpecializationDto
        {
            Id = spec.Id,
            Name = spec.Name,
            Description = spec.Description,
            CategoryId = spec.CategoryId,
            SubCategoryId = spec.SubCategoryId,
            DisplayOrder = spec.DisplayOrder,
            IsActive = spec.IsActive
        }, "تمت إضافة التخصص بنجاح"));
    }

    /// <summary>
    /// تعديل تخصص
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSpecializationDto dto)
    {
        var spec = await _context.Specializations.FindAsync(id);
        if (spec == null) return NotFound(ApiResponse<bool>.Fail("التخصص غير موجود"));

        spec.Name = dto.Name.Trim();
        spec.Description = dto.Description;
        spec.CategoryId = dto.CategoryId;
        spec.SubCategoryId = dto.SubCategoryId;
        spec.DisplayOrder = dto.DisplayOrder;
        spec.IsActive = dto.IsActive;
        spec.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true, "تم تحديث التخصص بنجاح"));
    }

    /// <summary>
    /// حذف تخصص (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var spec = await _context.Specializations.FindAsync(id);
        if (spec == null) return NotFound(ApiResponse<bool>.Fail("التخصص غير موجود"));

        spec.IsDeleted = true;
        spec.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true, "تم حذف التخصص بنجاح"));
    }

    /// <summary>
    /// زراعة المواد الدراسية المصرية الأساسية مسبقاً إذا كان الجدول فارغاً
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedDefaultSpecializations()
    {
        var count = await _context.Specializations.CountAsync();
        if (count > 0)
        {
            return Ok(ApiResponse<int>.Succeed(count, "الجدول يحتوي بالفعل على تخصصات"));
        }

        // ابحث عن تصنيفات المدرسين أو المراحل التعليمية لربطها
        var subCats = await _context.SubCategories.ToListAsync();
        var primarySub = subCats.FirstOrDefault(s => s.Name.Contains("ابتدائي"));
        var prepSub = subCats.FirstOrDefault(s => s.Name.Contains("إعدادي") || s.Name.Contains("اعدادي"));
        var secSub = subCats.FirstOrDefault(s => s.Name.Contains("ثانوي"));
        var teacherCat = await _context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("مدرس") || c.Name.Contains("تعليم"));

        var commonSubjects = new[]
        {
            "لغة عربية", "رياضيات", "لغة إنجليزية", "علوم", "دراسات اجتماعية",
            "تربية دينية إسلامية", "تربية دينية مسيحية", "لغة فرنسية", "لغة ألمانية",
            "حاسوب وتكنولوجيا المعلومات", "تأسيس ومتابعة شاملة"
        };

        var primarySubjects = new[]
        {
            "لغة عربية - ابتدائي", "حساب ورياضيات - ابتدائي", "لغة إنجليزية (Connect) - ابتدائي",
            "علوم - ابتدائي", "دراسات اجتماعية - ابتدائي", "تأسيس قراءة وكتابة", "تأسيس فونكس وماث"
        };

        var prepSubjects = new[]
        {
            "لغة عربية - إعدادي", "جبر وهندسة - إعدادي", "لغة إنجليزية - إعدادي",
            "علوم - إعدادي", "دراسات اجتماعية - إعدادي", "لغة فرنسية - إعدادي"
        };

        var secSubjects = new[]
        {
            "لغة عربية - ثانوي", "لغة إنجليزية - ثانوي", "فيزياء", "كيمياء", "أحياء",
            "جيولوجيا وعلوم بيئية", "رياضيات بحتة (تفاضل وتكامل وجبر)", "رياضيات تطبيقية (استاتيكا وديناميكا)",
            "تاريخ - ثانوي", "جغرافيا - ثانوي", "فلسفة ومنطق", "علم نفس واجتماع",
            "لغة فرنسية - ثانوي", "لغة ألمانية - ثانوي", "لغة إيطالية - ثانوي"
        };

        var azharSubjects = new[]
        {
            "قرآن كريم وتجويد", "فقه إسلامي", "تفسير وحديث", "توحيد وسيرة نبوية", "نحو وصرف وبلاغة"
        };

        var order = 1;
        var toAdd = new List<Specialization>();

        foreach (var sub in commonSubjects)
        {
            toAdd.Add(new Specialization(sub, teacherCat?.Id, null, order++));
        }

        foreach (var sub in primarySubjects)
        {
            toAdd.Add(new Specialization(sub, teacherCat?.Id, primarySub?.Id, order++));
        }

        foreach (var sub in prepSubjects)
        {
            toAdd.Add(new Specialization(sub, teacherCat?.Id, prepSub?.Id, order++));
        }

        foreach (var sub in secSubjects)
        {
            toAdd.Add(new Specialization(sub, teacherCat?.Id, secSub?.Id, order++));
        }

        foreach (var sub in azharSubjects)
        {
            toAdd.Add(new Specialization(sub, teacherCat?.Id, null, order++));
        }

        _context.Specializations.AddRange(toAdd);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<int>.Succeed(toAdd.Count, $"تمت زراعة {toAdd.Count} تخصصاً ومادة دراسية بنجاح"));
    }
}
