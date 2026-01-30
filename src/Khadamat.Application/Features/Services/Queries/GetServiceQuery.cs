using MediatR;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;

namespace Khadamat.Application.Features.Services.Queries;

public record GetServiceQuery : IRequest<PaginatedResult<ServiceDto>>
{
    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public int? SubCategoryId { get; init; }
    public int? GovernorateId { get; init; }
    public int? CityId { get; init; }
    public string? Location { get; init; }
    public bool? IsApproved { get; init; }
    public string? SortBy { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
