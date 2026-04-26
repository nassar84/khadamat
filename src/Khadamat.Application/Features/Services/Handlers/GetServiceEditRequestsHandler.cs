using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Application.Features.Services.Queries;
using Khadamat.Domain.Entities;
using MediatR;

namespace Khadamat.Application.Features.Services.Handlers;

public class GetServiceEditRequestsHandler : IRequestHandler<GetServiceEditRequestsQuery, List<ServiceEditRequestDto>>
{
    private readonly IGenericRepository<ServiceEditRequest> _requestRepo;
    private readonly IUserService _userService;

    public GetServiceEditRequestsHandler(IGenericRepository<ServiceEditRequest> requestRepo, IUserService userService)
    {
        _requestRepo = requestRepo;
        _userService = userService;
    }

    public async Task<List<ServiceEditRequestDto>> Handle(GetServiceEditRequestsQuery request, CancellationToken cancellationToken)
    {
        var editRequests = await _requestRepo.ListAllAsync(includeProperties: "Service");
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            editRequests = editRequests.Where(r => r.Status == request.Status).ToList();
        }

        // Batch get user info
        var userIds = editRequests.Select(r => r.RequesterId).Distinct().ToList();
        var userInfos = await _userService.GetUsersBasicInfoAsync(userIds);

        var results = new List<ServiceEditRequestDto>();

        foreach (var req in editRequests)
        {
             userInfos.TryGetValue(req.RequesterId, out var info);
            
            results.Add(new ServiceEditRequestDto
            {
                Id = req.Id,
                ServiceId = req.ServiceId,
                ServiceName = req.Service?.Name ?? "خدمة غير معروفة",
                RequesterId = req.RequesterId,
                RequesterName = info.Name ?? "مستخدم",
                Reason = req.Reason,
                
                CurrentName = req.Service?.Name ?? "",
                CurrentDescription = req.Service?.Description ?? "",
                CurrentAddress = req.Service?.Address ?? "",
                CurrentPrice = req.Service?.Price,
                CurrentPhone1 = req.Service?.Phone1,

                ProposedName = req.ProposedName,
                ProposedDescription = req.ProposedDescription,
                ProposedAddress = req.ProposedAddress,
                ProposedPrice = req.ProposedPrice,
                ProposedPhone1 = req.ProposedPhone1,
                ProposedPhone2 = req.ProposedPhone2,
                ProposedWhatsApp = req.ProposedWhatsApp,
                
                Status = req.Status,
                AdminNotes = req.AdminNotes,
                ProviderNotes = req.ProviderNotes,
                CreatedAt = req.CreatedAt,
                
                ApprovedName = req.ApprovedName,
                ApprovedDescription = req.ApprovedDescription,
                ApprovedAddress = req.ApprovedAddress,
                ApprovedPrice = req.ApprovedPrice,
                ApprovedPhone1 = req.ApprovedPhone1,
                
                HasOwner = !string.IsNullOrEmpty(req.Service?.UserCreated),
                OwnerUserId = req.Service?.UserCreated
            });
        }

        return results.OrderByDescending(r => r.CreatedAt).ToList();
    }
}
