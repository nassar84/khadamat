using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using System.Security.Claims;
using Khadamat.Application.Interfaces;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/posts")]
public class PostsController : ControllerBase
{
    private readonly KhadamatDbContext _context;
    private readonly IUserService _userService;

    public PostsController(KhadamatDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<List<PublicPostDto>>>> GetPublicPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var posts = await _context.Posts
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Include(p => p.Provider)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var providerUserIds = posts.Select(p => p.Provider?.UserId).Where(u => u != null).Distinct().Cast<string>().ToList();
        var providerUserDict = providerUserIds.Any()
            ? await _userService.GetUsersBasicInfoAsync(providerUserIds)
            : new Dictionary<string, (string Name, string Avatar)>();

        var postDtos = posts.Select(p =>
        {
            var providerName = !string.IsNullOrEmpty(p.Provider?.BusinessName)
                ? p.Provider.BusinessName
                : (p.Provider?.UserId != null && providerUserDict.TryGetValue(p.Provider.UserId, out var u) ? u.Name : "مقدم خدمة");

            return new PublicPostDto
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                LikesCount = p.Likes?.Count ?? 0,
                CommentsCount = p.Comments?.Count ?? 0,
                ProviderId = p.ProviderId,
                ProviderName = providerName ?? "مقدم خدمة",
                ProviderPhoto = p.Provider?.Photo
            };
        }).ToList();

        return Ok(ApiResponse<List<PublicPostDto>>.Succeed(postDtos));
    }

    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<ApiResponse<List<PostDto>>>> GetProviderPosts(int providerId)
    {
        var posts = await _context.Posts
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Where(p => p.ProviderId == providerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var commentUserIds = posts.SelectMany(p => p.Comments).Select(c => c.UserId).Distinct().ToList();
        var commentUserDict = await _userService.GetUsersBasicInfoAsync(commentUserIds);

        var postDtos = posts.Select(p => new PostDto
        {
            Id = p.Id,
            Content = p.Content,
            ImageUrl = p.ImageUrl,
            CreatedAt = p.CreatedAt,
            LikesCount = p.Likes.Count,
            CommentsCount = p.Comments.Count,
            Comments = p.Comments.Select(c =>
            {
                commentUserDict.TryGetValue(c.UserId, out var cUser);
                return new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    UserName = !string.IsNullOrEmpty(cUser.Name) ? cUser.Name : "مستخدم"
                };
            }).OrderByDescending(c => c.CreatedAt).ToList()
        }).ToList();

        return Ok(ApiResponse<List<PostDto>>.Succeed(postDtos));
    }

    [HttpPost]
    [Authorize]
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

    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var post = await _context.Posts.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

        var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);
        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Succeed(false));
        }
        else
        {
            var like = new Like(userId, postId: id);
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Succeed(true));
        }
    }
}

public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
