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

        // Notify provider that customer cancelled
        try
        {
            var providerProfile = await _context.ProviderProfiles.FindAsync(new object[] { serviceRequest.ProviderId }, cancellationToken);
            if (providerProfile != null && !string.IsNullOrEmpty(providerProfile.UserId))
            {
                var notif = new Domain.Entities.Notification(
                    providerProfile.UserId,
                    "تم إلغاء الطلب",
                    "قام العميل بإلغاء طلب الخدمة المعلق.",
                    "Order",
                    "/provider/incoming-requests"
                );
                _context.Notifications.Add(notif);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Failed to notify provider of cancellation: {ex.Message}");
        }

        return ApiResponse<bool>.Succeed(true);
    }
}
