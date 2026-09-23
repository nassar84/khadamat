using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/contact")]
public class ContactController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public ContactController(KhadamatDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Submit a contact/complaint message — open to all (including guests).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] ContactMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { success = false, message = "يرجى ملء جميع الحقول المطلوبة" });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var msg = new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Subject = request.Subject?.Trim() ?? string.Empty,
            SubjectType = request.SubjectType ?? "other",
            Message = request.Message.Trim(),
            UserId = userId
        };

        _context.ContactMessages.Add(msg);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "تم إرسال رسالتك بنجاح" });
    }

    /// <summary>
    /// Admin: list all contact messages (optionally filter by resolved status).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] bool? resolved = null)
    {
        var query = _context.ContactMessages
            .Where(m => !m.IsDeleted)
            .AsQueryable();

        if (resolved.HasValue)
            query = query.Where(m => m.IsResolved == resolved.Value);

        var list = await query
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Email,
                m.Subject,
                m.SubjectType,
                m.Message,
                m.IsResolved,
                m.ResolvedAt,
                m.AdminNotes,
                m.CreatedAt,
                m.UserId
            })
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>
    /// Admin: mark a message as resolved.
    /// </summary>
    [HttpPut("{id}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveRequest? req)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg == null || msg.IsDeleted) return NotFound();

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        msg.IsResolved = true;
        msg.ResolvedAt = DateTime.UtcNow;
        msg.ResolvedByUserId = adminId;
        msg.AdminNotes = req?.Notes;

        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}

public record ContactMessageRequest(
    string Name,
    string Email,
    string? Subject,
    string? SubjectType,
    string Message
);

public record ResolveRequest(string? Notes);
