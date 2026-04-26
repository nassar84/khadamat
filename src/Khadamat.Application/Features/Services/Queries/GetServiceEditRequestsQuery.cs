using Khadamat.Application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Khadamat.Application.Features.Services.Queries;

public class GetServiceEditRequestsQuery : IRequest<List<ServiceEditRequestDto>>
{
    public string? Status { get; set; } // Pending, Approved, etc.
}
