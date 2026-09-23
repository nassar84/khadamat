using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.Features.Requests.Commands;
using Khadamat.Application.Features.Requests.Queries;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Khadamat.Infrastructure.Persistence;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/requests")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly KhadamatDbContext _context;

    public RequestsController(IMediator mediator, KhadamatDbContext context)
    {
        _mediator = mediator;
        _context = context;
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
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<int>.Fail("يجب تسجيل الدخول أولاً لطلب الخدمة"));
        }

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

        // Get provider profile
        var provider = await _context.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (provider == null)
        {
            return Ok(new List<ServiceRequestDto>());
        }

        var result = await _mediator.Send(new GetProviderRequestsQuery(provider.Id));
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateRequestStatusCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var provider = await _context.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (provider == null)
        {
            return Forbid();
        }

        command.RequestId = id;
        command.ProviderId = provider.Id;
        
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
