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
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.ProviderId == request.ProviderId, cancellationToken);

        if (serviceRequest == null)
            return ApiResponse<bool>.Fail("الطلب غير موجود أو لا تملك صلاحية تعديله");

        serviceRequest.Status = request.Status;
        serviceRequest.ProviderNotes = request.ProviderNotes;
        serviceRequest.UpdatedAt = System.DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Notify customer of status change
        try
        {
            string statusText = request.Status switch
            {
                Domain.Enums.RequestStatus.Accepted => "تم قبول طلبك",
                Domain.Enums.RequestStatus.Rejected => "تم الاعتذار عن طلبك",
                Domain.Enums.RequestStatus.Completed => "تم إكمال طلبك بنجاح! شاركنا تقييمك للخدمة ومقدمها ⭐",
                _ => "تم تحديث حالة طلبك"
            };

            var notif = new Domain.Entities.Notification(
                serviceRequest.UserId,
                request.Status == Domain.Enums.RequestStatus.Completed ? "طلب مكتمل - قيم تجربتك" : statusText,
                $"طلبك لخدمة {serviceRequest.Service?.Name ?? "الخدمة"}: {statusText}",
                "Order",
                "/user/my-requests"
            );
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (System.Exception notifEx)
        {
            System.Console.WriteLine($"Failed to notify customer: {notifEx.Message}");
        }

        return ApiResponse<bool>.Succeed(true);
    }
}
