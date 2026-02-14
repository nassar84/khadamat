using MediatR;
using Khadamat.Application.Common.Models;

namespace Khadamat.Application.Features.Requests.Commands;

public record CancelRequestCommand : IRequest<ApiResponse<bool>>
{
    public int RequestId { get; init; }
    public string UserId { get; set; } = string.Empty;
}
