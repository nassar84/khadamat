using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.DTOs;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.Common.Models;
using System.Security.Claims;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public LocationsController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpGet("governorates")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GovernorateDto>>>> GetGovernorates()
    {
        var governorates = await _context.Governorates
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new GovernorateDto
            {
                Id = g.Id,
                NameAr = g.Governorate_Name_AR,
                NameEn = g.Governorate_Name_EN,
                DisplayOrder = g.DisplayOrder,
                Approved = g.Approved,
                CreatedAt = g.CreatedAt
            })
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<GovernorateDto>>.Succeed(governorates));
    }

    [HttpGet("governorates/{governorateId}/cities")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CityDto>>>> GetCities(int governorateId)
    {
        var cities = await _context.Cities
            .Where(c => c.GovernorateId == governorateId)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CityDto
            {
                Id = c.Id,
                GovernorateId = c.GovernorateId,
                NameAr = c.City_Name_AR,
                NameEn = c.City_Name_EN,
                DisplayOrder = c.DisplayOrder,
                Approved = c.Approved,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<CityDto>>.Succeed(cities));
    }

    [HttpPost("governorates")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateGovernorate(GovernorateDto dto)
    {
        var governorate = new Governorate
        {
            Governorate_Name_AR = dto.NameAr,
            Governorate_Name_EN = dto.NameEn,
            DisplayOrder = dto.DisplayOrder,
            Approved = dto.Approved,
            UserCreated = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };
        _context.Governorates.Add(governorate);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<int>.Succeed(governorate.Id));
    }

    [HttpPut("governorates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateGovernorate(int id, GovernorateDto dto)
    {
        var g = await _context.Governorates.FindAsync(id);
        if (g == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        g.Governorate_Name_AR = dto.NameAr;
        g.Governorate_Name_EN = dto.NameEn;
        g.DisplayOrder = dto.DisplayOrder;
        g.Approved = dto.Approved;
        g.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("governorates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteGovernorate(int id)
    {
        var g = await _context.Governorates.FindAsync(id);
        if (g == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        _context.Governorates.Remove(g);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("cities")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateCity(CityDto dto)
    {
        var city = new City
        {
            GovernorateId = dto.GovernorateId,
            City_Name_AR = dto.NameAr,
            City_Name_EN = dto.NameEn,
            DisplayOrder = dto.DisplayOrder,
            Approved = dto.Approved,
            UserCreated = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<int>.Succeed(city.Id));
    }

    [HttpPut("cities/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateCity(int id, CityDto dto)
    {
        var c = await _context.Cities.FindAsync(id);
        if (c == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        c.GovernorateId = dto.GovernorateId;
        c.City_Name_AR = dto.NameAr;
        c.City_Name_EN = dto.NameEn;
        c.DisplayOrder = dto.DisplayOrder;
        c.Approved = dto.Approved;
        c.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("cities/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCity(int id)
    {
        var c = await _context.Cities.FindAsync(id);
        if (c == null) return NotFound(ApiResponse<bool>.Fail("Not found"));
        
        _context.Cities.Remove(c);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }
}
