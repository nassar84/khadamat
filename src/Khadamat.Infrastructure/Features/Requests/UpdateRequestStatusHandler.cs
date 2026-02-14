using MediatR;
using Khadamat.Application.Common.Models;
using Khadamat.Application.Features.Requests.Commands;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Khadamat.Infrastructure.Features.Requests;

public class UpdateRequestStatusHandler : IRequestHandler<UpdateRequestStatusCommand, ApiResponse<bool>>
{
    private readonly KhadamatDbContext _context;

    public UpdateRequestStatusHandler(KhadamatDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.ProviderId == request.ProviderId, cancellationToken);

        if (serviceRequest == null)
            return ApiResponse<bool>.Fail("Request not found or you don't have permission");

        serviceRequest.Status = request.Status;
        serviceRequest.ProviderNotes = request.ProviderNotes;
        serviceRequest.UpdatedAt = System.DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Succeed(true);
    }
}
