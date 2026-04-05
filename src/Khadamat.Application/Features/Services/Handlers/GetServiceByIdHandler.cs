using MediatR;
using AutoMapper;
using Khadamat.Application.DTOs;
using Khadamat.Application.Features.Services.Queries;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using System.Linq;

namespace Khadamat.Application.Features.Services.Handlers;

public class GetServiceByIdHandler : IRequestHandler<GetServiceByIdQuery, ServiceDto?>
{
    private readonly IGenericRepository<Service> _serviceRepo;
    private readonly IGenericRepository<ProviderProfile> _providerRepo;
    private readonly IGenericRepository<Post> _postRepo;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public GetServiceByIdHandler(
        IGenericRepository<Service> serviceRepo,
        IGenericRepository<ProviderProfile> providerRepo,
        IGenericRepository<Post> postRepo,
        IMapper mapper,
        IUserService userService)
    {
        _serviceRepo = serviceRepo;
        _providerRepo = providerRepo;
        _postRepo = postRepo;
        _mapper = mapper;
        _userService = userService;
    }

    public async Task<ServiceDto?> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        string includes = "Category,Category.MainCategory,SubCategory,Ratings,SubCategory.Category,SubCategory.Category.MainCategory,City,City.Governorate";
        
        var services = await _serviceRepo.GetPagedAsync(1, 1, 
            filter: s => s.Id == request.Id, 
            includeProperties: includes);
            
        var service = services.FirstOrDefault();

        if (service == null) return null;

        var dto = _mapper.Map<ServiceDto>(service);
        
        // Map City and Governorate information
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

        // Explicitly map hierarchy
        if (service.SubCategory != null)
        {
            dto.SubCategoryId = service.SubCategoryId;
            dto.SubCategoryName = service.SubCategory.Name;

            if (service.SubCategory.Category != null)
            {
                dto.CategoryId = service.SubCategory.CategoryId;
                dto.CategoryName = service.SubCategory.Category.Name;

                if (service.SubCategory.Category.MainCategory != null)
                {
                    dto.MainCategoryId = service.SubCategory.Category.MainCategoryId;
                    dto.MainCategoryName = service.SubCategory.Category.MainCategory.Name;
                }
            }
        }
        else if (service.Category != null)
        {
            dto.CategoryId = service.CategoryId;
            dto.CategoryName = service.Category.Name;

            if (service.Category.MainCategory != null)
            {
                dto.MainCategoryId = service.Category.MainCategoryId;
                dto.MainCategoryName = service.Category.MainCategory.Name;
            }
        }

        // Fetch Provider Name & Photo
        var provider = await _providerRepo.GetByIdAsync(service.ProviderProfileId);

        if (provider != null)
        {
            dto.ProviderName = provider.BusinessName ?? service.Name;
            dto.ProviderPhoto = provider.Photo;
            
            // Fetch Posts
            // Posts have ProviderId (int) which matches ProviderProfile.Id
            var posts = await _postRepo.GetPagedAsync(1, 5, 
                filter: p => p.ProviderId == provider.Id,
                orderBy: q => q.OrderByDescending(x => x.CreatedAt),
                includeProperties: "Likes");
            
            dto.Posts = _mapper.Map<List<PostDto>>(posts);
        }

        // Map Ratings to Reviews manually if Mapper didn't do it (Mapper handles basic mapping but customization here is fine)
        if (service.Ratings != null && service.Ratings.Any())
        {
            var userIds = service.Ratings.Select(r => r.UserId).Distinct().ToList();
            var userDict = await _userService.GetUsersBasicInfoAsync(userIds);

            dto.Reviews = service.Ratings.Select(r => {
                userDict.TryGetValue(r.UserId, out var userInfo);
                return new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Stars,
                    Comment = r.Comment,
                    CreatedAt = r.Date,
                    UserName = !string.IsNullOrEmpty(userInfo.Name) ? userInfo.Name : "مستخدم بدون اسم",
                    UserAvatar = userInfo.Avatar
                };
            }).OrderByDescending(r => r.CreatedAt).ToList();
            
            dto.Rating = service.Ratings.Average(r => r.Stars);
            dto.RatersCount = service.Ratings.Count;
        }

        return dto;
    }
}
