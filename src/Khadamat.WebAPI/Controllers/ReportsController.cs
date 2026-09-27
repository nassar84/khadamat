using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.Common.Models;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using System.Security.Claims;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public ReportsController(KhadamatDbContext context) => _context = context;

    // ── User: submit a report ─────────────────────────────────────────────
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SubmitReport([FromBody] SubmitReportRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(ApiResponse<bool>.Fail("يجب تحديد سبب الإبلاغ"));

        // Prevent duplicate pending reports from the same user
        var exists = await _context.ContentReports.AnyAsync(r =>
            r.TargetType == request.TargetType &&
            r.TargetId   == request.TargetId   &&
            r.ReporterId == userId             &&
            r.Status     == ReportStatus.Pending);

        if (exists)
            return BadRequest(ApiResponse<bool>.Fail("لقد أبلغت عن هذا المحتوى مسبقاً وهو قيد المراجعة"));

        var report = new ContentReport(request.TargetType, request.TargetId, userId, request.Reason);
        _context.ContentReports.Add(report);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // ── Admin: list all reports ───────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<List<ContentReportDto>>>> GetReports(
        [FromQuery] ReportStatus? status,
        [FromQuery] ReportTargetType? targetType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        var query = _context.ContentReports.AsQueryable();
        if (status.HasValue)     query = query.Where(r => r.Status     == status.Value);
        if (targetType.HasValue) query = query.Where(r => r.TargetType == targetType.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        // Enrich reporter names
        var uids = items.Select(r => r.ReporterId).Distinct().ToList();
        var users = await _context.Users.Where(u => uids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName ?? "مستخدم");

        // Enrich target content (text preview)
        var reviewIds  = items.Where(r => r.TargetType == ReportTargetType.Review) .Select(r => r.TargetId).ToList();
        var commentIds = items.Where(r => r.TargetType == ReportTargetType.Comment).Select(r => r.TargetId).ToList();
        var reviews  = await _context.Ratings .Where(r => reviewIds .Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.Comment);
        var comments = await _context.Comments.Where(c => commentIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Text);

        var dtos = items.Select(r => new ContentReportDto
        {
            Id           = r.Id,
            TargetType   = r.TargetType,
            TargetId     = r.TargetId,
            ReporterId   = r.ReporterId,
            ReporterName = users.TryGetValue(r.ReporterId, out var n) ? n : "مستخدم",
            Reason       = r.Reason,
            Status       = r.Status,
            AdminNote    = r.AdminNote,
            CreatedAt    = r.CreatedAt,
            ResolvedAt   = r.ResolvedAt,
            TargetPreview = r.TargetType == ReportTargetType.Review
                ? (reviews.TryGetValue(r.TargetId,  out var rv) ? rv : "—")
                : (comments.TryGetValue(r.TargetId, out var cm) ? cm : "—")
        }).ToList();

        return Ok(ApiResponse<List<ContentReportDto>>.Succeed(dtos));
    }

    // ── Admin: resolve (delete target) ───────────────────────────────────
    [HttpPost("{id}/resolve")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> ResolveReport(int id, [FromBody] AdminActionRequest request)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var report  = await _context.ContentReports.FindAsync(id);
        if (report == null) return NotFound();

        // Delete the offending content
        if (report.TargetType == ReportTargetType.Review)
        {
            var review = await _context.Ratings.FindAsync(report.TargetId);
            if (review != null) _context.Ratings.Remove(review);
        }
        else
        {
            var comment = await _context.Comments.FindAsync(report.TargetId);
            if (comment != null) _context.Comments.Remove(comment);
        }

        report.Resolve(adminId, request.Note);

        // Also resolve other pending reports for the same target
        var others = await _context.ContentReports
            .Where(r => r.TargetType == report.TargetType &&
                        r.TargetId   == report.TargetId   &&
                        r.Status     == ReportStatus.Pending &&
                        r.Id         != id)
            .ToListAsync();
        foreach (var o in others) o.Resolve(adminId, "تم الحل بواسطة بلاغ مرتبط");

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // ── Admin: dismiss (keep content, close report) ───────────────────────
    [HttpPost("{id}/dismiss")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> DismissReport(int id, [FromBody] AdminActionRequest request)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var report  = await _context.ContentReports.FindAsync(id);
        if (report == null) return NotFound();

        report.Dismiss(adminId, request.Note);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // ── Admin: direct edit review ─────────────────────────────────────────
    [HttpPut("review/{id}")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> AdminEditReview(int id, [FromBody] AdminEditContentRequest request)
    {
        var review = await _context.Ratings.FindAsync(id);
        if (review == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Text)) review.Comment = request.Text;
        if (request.Stars.HasValue && request.Stars >= 1 && request.Stars <= 5)
            review.Stars = request.Stars.Value;

        review.Date = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // ── Admin: direct edit comment ────────────────────────────────────────
    [HttpPut("comment/{id}")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> AdminEditComment(int id, [FromBody] AdminEditContentRequest request)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Text))
            comment.UpdateText(request.Text);

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // ── Admin: direct delete review ───────────────────────────────────────
    [HttpDelete("review/{id}")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> AdminDeleteReview(int id)
    {
        var review = await _context.Ratings.FindAsync(id);
        if (review == null) return NotFound();
        _context.Ratings.Remove(review);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // ── Admin: direct delete comment ──────────────────────────────────────
    [HttpDelete("comment/{id}")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> AdminDeleteComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────
public class SubmitReportRequest
{
    public ReportTargetType TargetType { get; set; }
    public int              TargetId   { get; set; }
    public string           Reason     { get; set; } = string.Empty;
}

public class AdminActionRequest
{
    public string? Note { get; set; }
}

public class AdminEditContentRequest
{
    public string? Text  { get; set; }
    public int?    Stars { get; set; }
}

public class ContentReportDto
{
    public int              Id            { get; set; }
    public ReportTargetType TargetType    { get; set; }
    public int              TargetId      { get; set; }
    public string           TargetPreview { get; set; } = string.Empty;
    public string           ReporterId    { get; set; } = string.Empty;
    public string           ReporterName  { get; set; } = string.Empty;
    public string           Reason        { get; set; } = string.Empty;
    public ReportStatus     Status        { get; set; }
    public string?          AdminNote     { get; set; }
    public DateTime         CreatedAt     { get; set; }
    public DateTime?        ResolvedAt    { get; set; }
}
