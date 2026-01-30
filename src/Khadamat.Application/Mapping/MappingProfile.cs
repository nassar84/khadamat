using AutoMapper;
using Khadamat.Domain.Entities;
using Khadamat.Application.DTOs;

namespace Khadamat.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Service, ServiceDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.SubCategoryName, opt => opt.MapFrom(s => s.SubCategory != null ? s.SubCategory.Name : string.Empty))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : (s.SubCategory != null ? s.SubCategory.Category.Name : string.Empty)))
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId ?? (s.SubCategory != null ? s.SubCategory.CategoryId : 0)))
            .ForMember(d => d.MainCategoryId, opt => opt.MapFrom(s => s.Category != null ? s.Category.MainCategoryId : (s.SubCategory != null ? s.SubCategory.Category.MainCategoryId : 0)))
            .ForMember(d => d.MainCategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.MainCategory.Name : (s.SubCategory != null ? s.SubCategory.Category.MainCategory.Name : string.Empty)))
            .ForMember(d => d.Images, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.ImageUrl) ? new List<string> { s.ImageUrl } : new List<string>()))
            .ForMember(d => d.WorkDays, opt => opt.MapFrom(s => s.Work_Days))
            .ForMember(d => d.WorkHours, opt => opt.MapFrom(s => s.Work_Houers))
            .ForMember(d => d.IsApproved, opt => opt.MapFrom(s => s.Approved))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Ratings.Any() ? s.Ratings.Average(r => r.Stars) : 0))
            .ForMember(d => d.RatersCount, opt => opt.MapFrom(s => s.Ratings.Count));
            
        CreateMap<Post, PostDto>()
            .ForMember(d => d.LikesCount, opt => opt.MapFrom(s => s.Likes.Count));
            
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.MainCategoryName, opt => opt.MapFrom(s => s.MainCategory.Name));
            
        CreateMap<SubCategory, SubCategoryDto>()
             .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
             .ForMember(d => d.MainCategoryName, opt => opt.MapFrom(s => s.Category.MainCategory.Name));
             
        CreateMap<MainCategory, MainCategoryDto>();

        CreateMap<Governorate, GovernorateDto>()
            .ForMember(d => d.NameAr, opt => opt.MapFrom(s => s.Governorate_Name_AR))
            .ForMember(d => d.NameEn, opt => opt.MapFrom(s => s.Governorate_Name_EN));

        CreateMap<City, CityDto>()
            .ForMember(d => d.NameAr, opt => opt.MapFrom(s => s.City_Name_AR))
            .ForMember(d => d.NameEn, opt => opt.MapFrom(s => s.City_Name_EN))
            .ForMember(d => d.GovernorateName, opt => opt.MapFrom(s => s.Governorate != null ? s.Governorate.Governorate_Name_AR : string.Empty));
    }
}
