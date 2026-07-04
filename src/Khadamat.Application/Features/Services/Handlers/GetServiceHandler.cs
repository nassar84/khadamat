using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Khadamat.Domain.Entities;
using Khadamat.Application.Interfaces;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using AutoMapper;
using System.Linq.Expressions;
using System;
using System.Linq;

namespace Khadamat.Application.Features.Services.Handlers;

public class GetServiceHandler : IRequestHandler<Queries.GetServiceQuery, PaginatedResult<ServiceDto>>
{
    private readonly IGenericRepository<Service> _repository;
    private readonly IGenericRepository<ProviderProfile> _providerRepo;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetServiceHandler(
        IGenericRepository<Service> repository, 
        IGenericRepository<ProviderProfile> providerRepo,
        IUserService userService,
        IMapper mapper)
    {
        _repository = repository;
        _providerRepo = providerRepo;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ServiceDto>> Handle(Queries.GetServiceQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var search = request.Search?.ToLower();

        Expression<Func<Service, bool>> filter = s => 
            (!request.IsApproved.HasValue || s.Approved == request.IsApproved.Value) &&
            (string.IsNullOrEmpty(search) || s.Name.ToLower().Contains(search) || s.Description.ToLower().Contains(search)) &&
            // Fix: If categoryId is provided, also check services through their sub-categories
            (!request.CategoryId.HasValue || s.CategoryId == request.CategoryId || (s.SubCategory != null && s.SubCategory.CategoryId == request.CategoryId)) &&
            (!request.SubCategoryId.HasValue || s.SubCategoryId == request.SubCategoryId) &&
            (!request.CityId.HasValue || s.CityId == request.CityId) &&
            (!request.GovernorateId.HasValue || (s.City != null && s.City.GovernorateId == request.GovernorateId)) &&
            (string.IsNullOrEmpty(request.Location) || (s.Address != null && s.Address.Contains(request.Location)));
        
        // Includes for mapping
        string includes = "Category,Category.MainCategory,SubCategory,SubCategory.Category,SubCategory.Category.MainCategory,City,City.Governorate,Ratings,Likes";

        Func<IQueryable<Service>, IOrderedQueryable<Service>> orderBy = request.SortBy switch
        {
            "price-asc"      => q => q.OrderBy(s => s.Price ?? decimal.MaxValue),
            "price-desc"     => q => q.OrderByDescending(s => s.Price ?? 0),
            "rating"         => q => q.OrderByDescending(s => s.Ratings.Any() ? s.Ratings.Average(r => (double?)r.Stars) : 0)
                                      .ThenByDescending(s => s.CreatedAt),
            "latest"         => q => q.OrderByDescending(s => s.CreatedAt),
            "display-order"  => q => q.OrderBy(s => s.DisplayOrder).ThenByDescending(s => s.CreatedAt),
            // Default: DisplayOrder first (admin-controlled priority), then by rating
            _                => q => q.OrderBy(s => s.DisplayOrder == 0 ? int.MaxValue : s.DisplayOrder)
                                      .ThenByDescending(s => s.Ratings.Any() ? s.Ratings.Average(r => (double?)r.Stars) : 0)
                                      .ThenByDescending(s => s.CreatedAt)
        };

        var pagedItems = await _repository.GetPagedAsync(page, pageSize, filter, 
            orderBy: orderBy, 
            includeProperties: includes);
            
        var totalCount = await _repository.CountAsync(filter);
        
        var dtos = _mapper.Map<List<ServiceDto>>(pagedItems);
        
        // Fetch Provider Details for all services in the page
        var providerProfileIds = pagedItems.Select(s => s.ProviderProfileId).Distinct().ToList();
        var providers = providerProfileIds.Any()
            ? await _providerRepo.GetPagedAsync(1, providerProfileIds.Count, p => providerProfileIds.Contains(p.Id))
            : new List<ProviderProfile>();
        var providersDict = providers.ToDictionary(p => p.Id, p => p);
        
        var userIds = providers.Select(p => p.UserId).Distinct().ToList();
        var userDict = userIds.Any()
            ? await _userService.GetUsersBasicInfoAsync(userIds)
            : new Dictionary<string, (string Name, string Avatar)>();
        
        // Map City, Governorate, and Provider information for each service
        foreach (var dto in dtos)
        {
            var service = pagedItems.FirstOrDefault(s => s.Id == dto.Id);
            if (service?.City != null)
            {
                dto.CityName = service.City.City_Name_AR;
                dto.CityNameEn = service.City.City_Name_EN;
                dto.GovernorateId = service.City.GovernorateId;
                
                if (service.City.Governorate != null)
                {
                    dto.GovernorateName = service.City.Governorate.Governorate_Name_AR;
                    dto.GovernorateNameEn = service.City.Governorate.Governorate_Name_EN;
                }
            }

            if (service != null && providersDict.TryGetValue(service.ProviderProfileId, out var provider))
            {
                userDict.TryGetValue(provider.UserId, out var userInfo);
                string actualUserName = !string.IsNullOrWhiteSpace(userInfo.Name) ? userInfo.Name : "مقدم خدمة";
                
                dto.ProviderName = !string.IsNullOrWhiteSpace(provider.BusinessName)
                    ? provider.BusinessName
                    : actualUserName;
                dto.ProviderPhoto = provider.Photo;
            }
        }
        
        return new PaginatedResult<ServiceDto>(dtos, totalCount, page, pageSize);
    }
}
