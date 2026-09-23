using MediatR;
using Khadamat.Application.Common.Models;
using Khadamat.Application.Features.Requests.Commands;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

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
        try
        {
            var service = await _context.Services.FindAsync(new object[] { request.ServiceId }, cancellationToken);
            if (service == null) return ApiResponse<int>.Fail("الخدمة المطلوبة غير موجودة");

            int providerId = service.ProviderProfileId;

            // Ensure ProviderId points to a valid existing ProviderProfile
            if (providerId <= 0 || !await _context.ProviderProfiles.AnyAsync(p => p.Id == providerId, cancellationToken))
            {
                var existingProvider = await _context.ProviderProfiles.FirstOrDefaultAsync(cancellationToken);
                if (existingProvider != null)
                {
                    providerId = existingProvider.Id;
                }
                else
                {
                    // Create a provider profile if none exists
                    var newProfile = new ProviderProfile
                    {
                        BusinessName = "مزود خدمة",
                        UserId = request.UserId,
                        Verified = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ProviderProfiles.Add(newProfile);
                    await _context.SaveChangesAsync(cancellationToken);
                    providerId = newProfile.Id;
                }
            }

            var serviceRequest = new ServiceRequest
            {
                ServiceId = request.ServiceId,
                ProviderId = providerId,
                UserId = request.UserId,
                Notes = request.Notes,
                PreferredDate = request.PreferredDate,
                Status = Domain.Enums.RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync(cancellationToken);

            // Notify Provider of new request
            try
            {
                var providerProfile = await _context.ProviderProfiles.FindAsync(new object[] { providerId }, cancellationToken);
                if (providerProfile != null && !string.IsNullOrEmpty(providerProfile.UserId))
                {
                    var notif = new Notification(
                        providerProfile.UserId,
                        "طلب خدمة جديد",
                        $"لديك طلب جديد لخدمة: {service.Name}",
                        "Order",
                        "/provider/incoming-requests"
                    );
                    _context.Notifications.Add(notif);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception notifEx)
            {
                Console.WriteLine($"Failed to send notification: {notifEx.Message}");
            }

            return ApiResponse<int>.Succeed(serviceRequest.Id);
        }
        catch (Exception ex)
        {
            return ApiResponse<int>.Fail($"فشل في إرسال الطلب: {ex.Message}");
        }
    }
}
