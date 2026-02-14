using MediatR;
using Khadamat.Application.Common.Models;
using Khadamat.Application.Features.Requests.Commands;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Khadamat.Infrastructure.Features.Requests;

public class CreateRequestHandler : IRequestHandler<CreateRequestCommand, ApiResponse<int>>
{
    private readonly KhadamatDbContext _context;

    public CreateRequestHandler(KhadamatDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<int>> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services.FindAsync(request.ServiceId);
        if (service == null) return ApiResponse<int>.Fail("Service not found");

        var serviceRequest = new ServiceRequest
        {
            ServiceId = request.ServiceId,
            ProviderId = service.ProviderProfileId,
            UserId = request.UserId,
            Notes = request.Notes,
            PreferredDate = request.PreferredDate,
            Status = Domain.Enums.RequestStatus.Pending,
            CreatedAt = System.DateTime.UtcNow
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<int>.Succeed(serviceRequest.Id);
    }
}
