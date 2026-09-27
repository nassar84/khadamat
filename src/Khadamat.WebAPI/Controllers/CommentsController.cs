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
[Route("v1/comments")]
public class CommentsController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public CommentsController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<MyCommentDto>>>> GetMyComments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var comments = await _context.Comments
            .Include(c => c.Post)
            .ThenInclude(p => p.Provider)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new MyCommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                PostContentSnippet = c.Post.Content.Length > 50 ? c.Post.Content.Substring(0, 50) + "..." : c.Post.Content,
                ProviderName = c.Post.Provider.BusinessName,
                Text = c.Text,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<MyCommentDto>>.Succeed(comments));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var comment = new Comment(request.PostId, userId, request.Text);
        
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<int>.Succeed(comment.Id));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> EditComment(int id, [FromBody] EditCommentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        if (comment.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(ApiResponse<bool>.Fail("نص التعليق لا يمكن أن يكون فارغاً"));

        comment.UpdateText(request.Text);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        if (comment.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // ── Admin: list all comments ──────────────────────────────────────────
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<List<AdminCommentDto>>>> AdminGetAllComments(
        [FromQuery] int? postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var query = _context.Comments.Include(c => c.Post).AsQueryable();
        if (postId.HasValue) query = query.Where(c => c.PostId == postId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new AdminCommentDto
            {
                Id        = c.Id,
                PostId    = c.PostId,
                UserId    = c.UserId,
                Text      = c.Text,
                CreatedAt = c.CreatedAt
            }).ToListAsync();

        var uids = items.Select(i => i.UserId).Distinct().ToList();
        var users = await _context.Users.Where(u => uids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName ?? "مستخدم");

        foreach (var item in items)
            item.UserName = users.TryGetValue(item.UserId, out var un) ? un : "مستخدم";

        return Ok(ApiResponse<List<AdminCommentDto>>.Succeed(items));
    }
}

public class EditCommentRequest
{
    public string Text { get; set; } = string.Empty;
}

public class AdminCommentDto
{
    public int      Id        { get; set; }
    public int      PostId    { get; set; }
    public string   UserId    { get; set; } = string.Empty;
    public string   UserName  { get; set; } = string.Empty;
    public string   Text      { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentRequest
{
    public int PostId { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class MyCommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string PostContentSnippet { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
