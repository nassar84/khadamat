using Khadamat.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("share/service/{id}/generate-card")]
public class ShareCardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShareCardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [HttpOptions]
    public async Task<IActionResult> GenerateCard(int id)
    {
        try
        {
            var service = await _mediator.Send(new GetServiceByIdQuery(id));
            if (service == null)
                return NotFound(new { success = false, message = "الخدمة غير موجودة" });

            return Ok(new
            {
                success = false,
                imageUrl = (string?)null,
                message = "يتم استخدام البطاقة من المتصفح"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShareCards] Error: {ex.Message}");
            return StatusCode(500, new { success = false, message = "حدث خطأ" });
        }
    }
}
