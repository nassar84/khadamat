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
    private readonly IMapper _mapper;

    public GetServiceHandler(IGenericRepository<Service> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ServiceDto>> Handle(Queries.GetServiceQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.ToLower();

        Expression<Func<Service, bool>> filter = s => 
            (!request.IsApproved.HasValue || s.Approved == request.IsApproved.Value) &&
            (string.IsNullOrEmpty(search) || s.Name.ToLower().Contains(search) || s.Description.ToLower().Contains(search)) &&
            (!request.CategoryId.HasValue || s.CategoryId == request.CategoryId) &&
            (!request.SubCategoryId.HasValue || s.SubCategoryId == request.SubCategoryId) &&
            (!request.CityId.HasValue || s.CityId == request.CityId) &&
            (!request.GovernorateId.HasValue || s.City.GovernorateId == request.GovernorateId) &&
            (string.IsNullOrEmpty(request.Location) || s.Address.Contains(request.Location));
        
        // Includes for mapping
        string includes = "Category,SubCategory,City,City.Governorate";

        Func<IQueryable<Service>, IOrderedQueryable<Service>> orderBy = request.SortBy switch
        {
            "price-asc" => q => q.OrderBy(s => s.Price ?? decimal.MaxValue),
            "rating" => q => q.OrderByDescending(s => s.Ratings.Any() ? s.Ratings.Average(r => (double?)r.Stars) : 0),
            _ => q => q.OrderByDescending(s => s.CreatedAt)
        };

        var pagedItems = await _repository.GetPagedAsync(request.Page, request.PageSize, filter, 
            orderBy: orderBy, 
            includeProperties: includes);
            
        var totalCount = await _repository.CountAsync(filter);
        
        var dtos = _mapper.Map<List<ServiceDto>>(pagedItems);
        
        // Map City and Governorate information for each service
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
        }
        
        return new PaginatedResult<ServiceDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
