using MediatR;
using Khadamat.Application.DTOs;

namespace Khadamat.Application.Features.Services.Queries;

public record GetServiceByIdQuery(int Id) : IRequest<ServiceDto?>;
