using MediatR;
using Khadamat.Application.Common.Models;
using Khadamat.Domain.Enums;

namespace Khadamat.Application.Features.Requests.Commands;

public record UpdateRequestStatusCommand : IRequest<ApiResponse<bool>>
{
    public int RequestId { get; set; }
    public RequestStatus Status { get; set; }
    public string? ProviderNotes { get; set; }
    public int ProviderId { get; set; }
}
