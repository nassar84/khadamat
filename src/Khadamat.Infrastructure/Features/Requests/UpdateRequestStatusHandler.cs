using MediatR;
using Khadamat.Application.Common.Models;
using Khadamat.Application.Features.Requests.Commands;
using Khadamat.Application.Interfaces;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Khadamat.Infrastructure.Features.Requests;

public class UpdateRequestStatusHandler : IRequestHandler<UpdateRequestStatusCommand, ApiResponse<bool>>
{
    private readonly KhadamatDbContext _context;
    private readonly INotificationService _notificationService;

    public UpdateRequestStatusHandler(KhadamatDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
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

        // Notify customer of status change (DB + Real-time SignalR)
        try
        {
            string statusText = request.Status switch
            {
                Domain.Enums.RequestStatus.Accepted => "تم قبول طلبك ✅",
                Domain.Enums.RequestStatus.Rejected => "تم الاعتذار عن طلبك ❌",
                Domain.Enums.RequestStatus.Completed => "تم إكمال طلبك بنجاح! شاركنا تقييمك للخدمة ومقدمها ⭐",
                _ => "تم تحديث حالة طلبك"
            };

            var title = request.Status == Domain.Enums.RequestStatus.Completed
                ? "طلب مكتمل - قيّم تجربتك ⭐"
                : statusText;

            await _notificationService.SendNotificationAsync(
                serviceRequest.UserId,
                title,
                $"طلبك لخدمة {serviceRequest.Service?.Name ?? "الخدمة"}: {statusText}",
                "Order",
                "/user/my-requests"
            );
        }
        catch (System.Exception notifEx)
        {
            System.Console.WriteLine($"Failed to notify customer: {notifEx.Message}");
        }

        return ApiResponse<bool>.Succeed(true);
    }
}
