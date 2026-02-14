using MediatR;
using Khadamat.Application.DTOs;
using System.Collections.Generic;

namespace Khadamat.Application.Features.Requests.Queries;

public record GetProviderRequestsQuery(int ProviderId) : IRequest<List<ServiceRequestDto>>;
