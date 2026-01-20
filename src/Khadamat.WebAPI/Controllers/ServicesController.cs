using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.Features.Services.Queries;
using Khadamat.Application.Features.Services.Commands;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
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
        
        return CreatedAtAction(nameof(GetServices), new { id = serviceId }, new { id = serviceId });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (id != command.Id) return BadRequest("ID mismatch");

        command.UserId = userId;
        var result = await _mediator.Send(command);
        
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("myservices")]
    [Authorize]
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
}
