using MediatR;
using Khadamat.Application.DTOs;
using System.Collections.Generic;

namespace Khadamat.Application.Features.Requests.Queries;

public record GetUserRequestsQuery(string UserId) : IRequest<List<ServiceRequestDto>>;
