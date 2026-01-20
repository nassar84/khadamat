using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using MediatR;

namespace Khadamat.Application.Features.Services.Queries;

public record GetProviderServicesQuery : IRequest<PaginatedResult<ServiceDto>>
{
    public string UserId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
