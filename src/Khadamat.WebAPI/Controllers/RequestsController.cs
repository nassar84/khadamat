using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Security.Claims;
using Khadamat.Application.Features.Requests.Commands;
using Khadamat.Application.Features.Requests.Queries;
using Khadamat.Application.DTOs;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/requests")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new GetUserRequestsQuery(userId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        command.UserId = userId;
        var result = await _mediator.Send(command);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    [HttpGet("provider-requests")]
    [Authorize]
    public async Task<IActionResult> GetProviderRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // Get provider profile ID
        var profile = await _mediator.Send(new GetUserRequestsQuery(userId)); // Temporary, need to get provider ID properly
        
        // For now, return empty list - will implement properly with provider profile lookup
        return Ok(new List<ServiceRequestDto>());
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateRequestStatusCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        command.RequestId = id;
        // Temporary: Should get provider ID from profile
        // For now, let's assume the providerId is passed in the command or derived from user
        
        var result = await _mediator.Send(command);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    [HttpPut("my-requests/{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelRequest(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var command = new CancelRequestCommand { RequestId = id, UserId = userId };
        
        var result = await _mediator.Send(command);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }
}
