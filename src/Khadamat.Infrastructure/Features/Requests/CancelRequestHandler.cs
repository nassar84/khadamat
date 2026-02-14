using MediatR;
using Khadamat.Application.Common.Models;
using Khadamat.Domain.Enums;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Application.Features.Requests.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Khadamat.Infrastructure.Features.Requests;

public class CancelRequestHandler : IRequestHandler<CancelRequestCommand, ApiResponse<bool>>
{
    private readonly KhadamatDbContext _context;

    public CancelRequestHandler(KhadamatDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(CancelRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.UserId == request.UserId, cancellationToken);

        if (serviceRequest == null)
            return ApiResponse<bool>.Fail("الطلب غير موجود أو ليس لديك الصلاحية لإلغائه.");

        if (serviceRequest.Status != RequestStatus.Pending)
            return ApiResponse<bool>.Fail("لا يمكن إلغاء الطلب إلا إذا كان قيد الانتظار.");

        serviceRequest.Status = RequestStatus.Cancelled;
        serviceRequest.UpdatedAt = System.DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Succeed(true);
    }
}
