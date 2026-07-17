using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.Features.Services.Queries;
using Khadamat.Application.Features.Services.Commands;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Infrastructure.Services;
using System.IO;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/services")]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly KhadamatDbContext _context;

    public ServicesController(IMediator mediator, KhadamatDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetServices([FromQuery] GetServiceQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceById(int id)
    {
        var result = await _mediator.Send(new GetServiceByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        command.UserId = userId;
        var serviceId = await _mediator.Send(command);
        
        // Handle service image renaming to subc_{CategoryId}_{ServiceId}.ext
        var service = await _context.Services.FindAsync(serviceId);
        if (service != null && !string.IsNullOrEmpty(service.ImageUrl))
        {
            var cleanImage = ImageNamingHelper.ExtractFileName(service.ImageUrl);
            if (!string.IsNullOrEmpty(cleanImage))
            {
                var categoryIdVal = service.SubCategoryId ?? service.CategoryId ?? 0;
                var targetName = $"s_{categoryIdVal}_{service.Id}";
                var finalName = ImageNamingHelper.RenameImage(cleanImage, "services", targetName);
                if (finalName != service.ImageUrl)
                {
                    service.SetImage(finalName);
                    await _context.SaveChangesAsync();
                }
            }
        }
        
        return CreatedAtAction(nameof(GetServices), new { id = serviceId }, new { id = serviceId });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireProvider")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (id != command.Id) return BadRequest("ID mismatch");

        // 1. Get the current service from DB (before updating) to track the old image name
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();
        
        var oldImageName = service.ImageUrl;

        // 2. Send the update command
        command.UserId = userId;
        var result = await _mediator.Send(command);
        
        if (!result) return NotFound();

        // 3. Rename the newly uploaded temp file and clean up the old one
        var updatedService = await _context.Services.FindAsync(id);
        if (updatedService != null && !string.IsNullOrEmpty(updatedService.ImageUrl))
        {
            var cleanNewImage = ImageNamingHelper.ExtractFileName(updatedService.ImageUrl);
            if (!string.IsNullOrEmpty(cleanNewImage))
            {
                // Only rename and clean up if the image has changed
                if (cleanNewImage != oldImageName)
                {
                    // Clean up the old image file if it exists
                    if (!string.IsNullOrEmpty(oldImageName))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "services", oldImageName);
                        if (System.IO.File.Exists(oldPath))
                        {
                            try { System.IO.File.Delete(oldPath); } catch { }
                        }
                    }

                    // Rename the new temp file to the standard format
                    var categoryIdVal = updatedService.SubCategoryId ?? updatedService.CategoryId ?? 0;
                    var targetName = $"s_{categoryIdVal}_{updatedService.Id}";
                    var finalName = ImageNamingHelper.RenameImage(cleanNewImage, "services", targetName);
                    
                    if (finalName != updatedService.ImageUrl)
                    {
                        updatedService.SetImage(finalName);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
        else if (updatedService != null && string.IsNullOrEmpty(updatedService.ImageUrl) && !string.IsNullOrEmpty(oldImageName))
        {
            // If the image was cleared, delete the old file
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "services", oldImageName);
            if (System.IO.File.Exists(oldPath))
            {
                try { System.IO.File.Delete(oldPath); } catch { }
            }
        }

        return NoContent();
    }

    [HttpGet("myservices")]
    [Authorize] // Changed from RequireProvider to allow applicants to see their pending services
    public async Task<IActionResult> GetMyServices([FromQuery] int page = 1)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var query = new GetProviderServicesQuery
        {
            UserId = userId,
            Page = page
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("{id}/similar")]
    public async Task<IActionResult> GetSimilarServices(int id, [FromQuery] int count = 4)
    {
        // For simplicity, using a direct query or MediatR if available.
        // I'll check if there's a Query for this. If not, I'll create one.
        // Since I don't want to create too many files, I'll use the existing GetServiceQuery with filters.
        var service = await _mediator.Send(new GetServiceByIdQuery(id));
        if (service == null) return NotFound();

        var query = new GetServiceQuery
        {
            CategoryId = service.CategoryId,
            SubCategoryId = service.SubCategoryId,
            PageSize = count + 1, // +1 to exclude current
            IsApproved = true
        };

        var result = await _mediator.Send(query);
        // Exclude current service
        result.Items = result.Items.Where(i => i.Id != id).Take(count).ToList();
        
        return Ok(result);
    }

    [HttpPost("{id}/request-edit")]
    [Authorize]
    public async Task<IActionResult> RequestEdit(int id, [FromBody] CreateServiceEditRequestCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (id != command.ServiceId) return BadRequest("ID mismatch");

        command.RequesterId = userId;
        var result = await _mediator.Send(command);
        
        if (!result) return NotFound();
        return Ok(new { message = "تم إرسال طلب التعديل بنجاح" });
    }
}
