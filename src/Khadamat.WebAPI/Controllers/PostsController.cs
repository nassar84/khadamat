using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using System.Security.Claims;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PostsController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public PostsController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<ApiResponse<List<PostDto>>>> GetProviderPosts(int providerId)
    {
        var posts = await _context.Posts
            .Where(p => p.ProviderId == providerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                LikesCount = p.Likes.Count
            })
            .ToListAsync();

        return Ok(ApiResponse<List<PostDto>>.Succeed(posts));
    }

    [HttpPost]
    [Authorize(Policy = "RequireProvider")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var provider = await _context.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (provider == null) return BadRequest("Provider profile not found");

        var post = new Post(provider.Id, request.Content, request.ImageUrl);
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<int>.Succeed(post.Id));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var post = await _context.Posts.Include(p => p.Provider).FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

        // Check ownership: Provider User ID must match
        if (post.Provider.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true));
    }
}

public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
