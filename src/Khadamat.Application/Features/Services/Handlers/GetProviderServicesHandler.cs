using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Khadamat.Domain.Entities;
using Khadamat.Application.Interfaces;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using AutoMapper;
using System.Linq;

namespace Khadamat.Application.Features.Services.Handlers;

public class GetProviderServicesHandler : IRequestHandler<Queries.GetProviderServicesQuery, PaginatedResult<ServiceDto>>
{
    private readonly IGenericRepository<Service> _repository;
    private readonly IGenericRepository<ProviderProfile> _providerRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetProviderServicesHandler(
        IGenericRepository<Service> repository, 
        IGenericRepository<ProviderProfile> providerRepository, 
        IUserService userService,
        IMapper mapper)
    {
        _repository = repository;
        _providerRepository = providerRepository;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ServiceDto>> Handle(Queries.GetProviderServicesQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetAsync(p => p.UserId == request.UserId);
        int providerId = provider?.Id ?? 0;

        string includes = "Category,Category.MainCategory,SubCategory,SubCategory.Category,SubCategory.Category.MainCategory,City,City.Governorate,Ratings,Likes";

        var pagedItems = await _repository.GetPagedAsync(request.Page, request.PageSize, 
            filter: s => s.ProviderProfileId == providerId || (!string.IsNullOrEmpty(request.UserId) && s.UserCreated == request.UserId), 
            orderBy: q => q.OrderByDescending(s => s.Ratings.Any() ? s.Ratings.Average(r => (double?)r.Stars) : 0)
                           .ThenByDescending(s => s.CreatedAt),
            includeProperties: includes);
            
        var totalCount = await _repository.CountAsync(s => s.ProviderProfileId == providerId || (!string.IsNullOrEmpty(request.UserId) && s.UserCreated == request.UserId));
        
        var dtos = _mapper.Map<List<ServiceDto>>(pagedItems);
        
        // Fetch Provider Name & Photo
        string actualUserName = "مقدم خدمة";
        if (provider != null)
        {
            var pUserDict = await _userService.GetUsersBasicInfoAsync(new List<string> { provider.UserId });
            if (pUserDict.TryGetValue(provider.UserId, out var pUserInfo) && !string.IsNullOrWhiteSpace(pUserInfo.Name))
            {
                actualUserName = pUserInfo.Name;
            }
        }

        string providerName = provider != null && !string.IsNullOrWhiteSpace(provider.BusinessName)
            ? provider.BusinessName
            : actualUserName;
        string providerPhoto = provider?.Photo ?? string.Empty;
        
        // Map City, Governorate, Provider, and Category hierarchy information for each service
        foreach (var dto in dtos)
        {
            var service = pagedItems.FirstOrDefault(s => s.Id == dto.Id);
            if (service != null)
            {
                if (service.City != null)
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

                // Explicitly map Category & SubCategory hierarchy and image URLs
                if (service.SubCategory != null)
                {
                    dto.SubCategoryId = service.SubCategoryId;
                    dto.SubCategoryName = service.SubCategory.Name;
                    dto.SubCategoryImageUrl = service.SubCategory.ImageUrl;

                    if (service.SubCategory.Category != null)
                    {
                        dto.CategoryId = service.SubCategory.CategoryId;
                        dto.CategoryName = service.SubCategory.Category.Name;
                        dto.CategoryImageUrl = service.SubCategory.Category.ImageUrl;

                        if (service.SubCategory.Category.MainCategory != null)
                        {
                            dto.MainCategoryId = service.SubCategory.Category.MainCategoryId;
                            dto.MainCategoryName = service.SubCategory.Category.MainCategory.Name;
                            dto.MainCategoryImageUrl = service.SubCategory.Category.MainCategory.ImageUrl;
                        }
                    }
                }
                else if (service.Category != null)
                {
                    dto.CategoryId = service.CategoryId;
                    dto.CategoryName = service.Category.Name;
                    dto.CategoryImageUrl = service.Category.ImageUrl;

                    if (service.Category.MainCategory != null)
                    {
                        dto.MainCategoryId = service.Category.MainCategoryId;
                        dto.MainCategoryName = service.Category.MainCategory.Name;
                        dto.MainCategoryImageUrl = service.Category.MainCategory.ImageUrl;
                    }
                }
            }

            dto.ProviderName = providerName;
            dto.ProviderPhoto = providerPhoto;
        }
        
        return new PaginatedResult<ServiceDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
