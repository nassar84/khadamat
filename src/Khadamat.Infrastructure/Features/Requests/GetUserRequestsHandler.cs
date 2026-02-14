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

public class GetUserRequestsHandler : IRequestHandler<GetUserRequestsQuery, List<ServiceRequestDto>>
{
    private readonly KhadamatDbContext _context;

    public GetUserRequestsHandler(KhadamatDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceRequestDto>> Handle(GetUserRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _context.ServiceRequests
            .Include(r => r.Service)
            .Include(r => r.Provider)
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(r => new ServiceRequestDto
        {
            Id = r.Id,
            UserId = r.UserId,
            ServiceId = r.ServiceId,
            ServiceTitle = r.Service.Name,
            ServiceIcon = GetIconFromCategory(r.Service.CategoryId),
            ProviderId = r.ProviderId,
            ProviderName = r.Provider.BusinessName,
            Status = r.Status,
            StatusText = GetStatusArabic(r.Status),
            Notes = r.Notes,
            ProviderNotes = r.ProviderNotes,
            RequestedAt = r.CreatedAt,
            PreferredDate = r.PreferredDate
        }).ToList();
    }

    private string GetIconFromCategory(int? categoryId) => categoryId switch
    {
        1 => "fa-heartbeat",      // صحة
        2 => "fa-wrench",          // صيانة
        3 => "fa-graduation-cap",  // تعليم
        4 => "fa-paint-roller",    // تشطيبات
        5 => "fa-car",             // سيارات
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
