using MediatR;
using Khadamat.Application.Common.Models;
using System;

namespace Khadamat.Application.Features.Requests.Commands;

public record CreateRequestCommand : IRequest<ApiResponse<int>>
{
    public int ServiceId { get; init; }
    public string? Notes { get; init; }
    public DateTime? PreferredDate { get; init; }
    public string UserId { get; set; } = string.Empty;
}
