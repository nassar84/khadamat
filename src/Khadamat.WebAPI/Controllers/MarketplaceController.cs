using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using System.Security.Claims;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketplaceController : ControllerBase
{
    private readonly IMarketplaceService _marketplaceService;

    public MarketplaceController(IMarketplaceService marketplaceService)
    {
        _marketplaceService = marketplaceService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetItem(int id)
    {
        var item = await _marketplaceService.GetItemByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] int count = 10)
    {
        var items = await _marketplaceService.GetLatestItemsAsync(count);
        return Ok(items);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 6)
    {
        try
        {
            var items = await _marketplaceService.GetFeaturedItemsAsync(count);
            return Ok(items);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MARKETPLACE ERROR (Featured): {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int? categoryId,
        [FromQuery] int? subCategoryId,
        [FromQuery] int? governorateId,
        [FromQuery] int? cityId,
        [FromQuery] string? condition,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sellerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var items = await _marketplaceService.SearchItemsAsync(q, categoryId, subCategoryId, governorateId, cityId, condition, minPrice, maxPrice, sellerId, page, pageSize);
        return Ok(items);
    }

    [Authorize]
    [HttpPost("{id}/favorite")]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _marketplaceService.ToggleFavoriteAsync(id, userId);
        return Ok();
    }

    [Authorize]
    [HttpGet("{id}/is-favorite")]
    public async Task<IActionResult> IsFavorite(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _marketplaceService.IsFavoriteAsync(id, userId);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateMarketplaceItemRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var item = await _marketplaceService.CreateItemAsync(request, userId);
            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner error: {ex.InnerException.Message}");
            }
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateMarketplaceItemRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _marketplaceService.UpdateItemAsync(id, request, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _marketplaceService.DeleteItemAsync(id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("my-items")]
    public async Task<IActionResult> GetMyItems([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var items = await _marketplaceService.SearchItemsAsync(null, null, null, null, null, null, null, null, userId, page, pageSize);
        return Ok(items);
    }

    [Authorize]
    [HttpPost("{id}/sold")]
    public async Task<IActionResult> MarkAsSold(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _marketplaceService.MarkAsSoldAsync(id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Admin Endpoints
    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromQuery] string? notes)
    {
        await _marketplaceService.ApproveItemAsync(id, notes);
        return Ok();
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromQuery] string? notes)
    {
        await _marketplaceService.RejectItemAsync(id, notes);
        return Ok();
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("{id}/set-featured")]
    public async Task<IActionResult> SetFeatured(int id, [FromQuery] int days = 7)
    {
        await _marketplaceService.SetFeaturedAsync(id, days);
        return Ok();
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("{id}/set-promoted")]
    public async Task<IActionResult> SetPromoted(int id, [FromQuery] int days = 7)
    {
        await _marketplaceService.SetPromotedAsync(id, days);
        return Ok();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _marketplaceService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MARKETPLACE ERROR (Categories): {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("categories/{categoryId}/subcategories")]
    public async Task<IActionResult> GetSubCategories(int categoryId)
    {
        var subCategories = await _marketplaceService.GetSubCategoriesAsync(categoryId);
        return Ok(subCategories);
    }
}
