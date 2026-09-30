using MediatR;
using Khadamat.Application.DTOs;
using Khadamat.Application.Features.Requests.Queries;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Khadamat.Infrastructure.Features.Requests;

public class GetProviderRequestsHandler : IRequestHandler<GetProviderRequestsQuery, List<ServiceRequestDto>>
{
    private readonly KhadamatDbContext _context;

    public GetProviderRequestsHandler(KhadamatDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceRequestDto>> Handle(GetProviderRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _context.ServiceRequests
            .Include(r => r.Service)
            .Include(r => r.Provider)
            .Include(r => r.Rating)
            .Where(r => r.ProviderId == request.ProviderId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var userIds = requests.Select(r => r.UserId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        return requests.Select(r => {
            users.TryGetValue(r.UserId, out var clientUser);
            var custName = !string.IsNullOrWhiteSpace(clientUser?.FullName) ? clientUser.FullName : (clientUser?.UserName ?? "عميل");
            return new ServiceRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                CustomerName = custName,
                CustomerPhone = clientUser?.PhoneNumber ?? string.Empty,
                CustomerPhoto = clientUser?.ProfileImageUrl,
                ServiceId = r.ServiceId,
                ServiceTitle = r.Service?.Name ?? "خدمة",
                ServiceIcon = GetIconFromCategory(r.Service?.CategoryId),
                ProviderId = r.ProviderId,
                ProviderName = r.Provider?.BusinessName ?? string.Empty,
                Status = r.Status,
                StatusText = GetStatusArabic(r.Status),
                Notes = r.Notes,
                ProviderNotes = r.ProviderNotes,
                RequestedAt = r.CreatedAt,
                PreferredDate = r.PreferredDate,
                HasRated = r.Rating != null,
                RatingStars = r.Rating?.Stars,
                RatingComment = r.Rating?.Comment
            };
        }).ToList();
    }

    private string GetIconFromCategory(int? categoryId) => categoryId switch
    {
        1 => "fa-heartbeat",
        2 => "fa-wrench",
        3 => "fa-graduation-cap",
        4 => "fa-paint-roller",
        5 => "fa-car",
        _ => "fa-briefcase"
    };

    private string GetStatusArabic(Domain.Enums.RequestStatus status) => status switch
    {
        Domain.Enums.RequestStatus.Pending => "في انتظار الموافقة",
        Domain.Enums.RequestStatus.Accepted => "تم القبول",
        Domain.Enums.RequestStatus.InProgress => "جاري التنفيذ",
        Domain.Enums.RequestStatus.Completed => "مكتمل",
        Domain.Enums.RequestStatus.Cancelled => "ملغي",
        Domain.Enums.RequestStatus.Rejected => "مرفوض",
        _ => "غير معروف"
    };
}
