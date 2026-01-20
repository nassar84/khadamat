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
    private readonly IMapper _mapper;

    public GetProviderServicesHandler(IGenericRepository<Service> repository, IGenericRepository<ProviderProfile> providerRepository, IMapper mapper)
    {
        _repository = repository;
        _providerRepository = providerRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ServiceDto>> Handle(Queries.GetProviderServicesQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetAsync(p => p.UserId == request.UserId);
        int providerId = provider?.Id ?? 0;

        string includes = "Category,SubCategory,City,City.Governorate";

        var pagedItems = await _repository.GetPagedAsync(request.Page, request.PageSize, 
            filter: s => s.ProviderProfileId == providerId, 
            orderBy: q => q.OrderByDescending(s => s.CreatedAt), 
            includeProperties: includes);
            
        var totalCount = await _repository.CountAsync(s => s.ProviderProfileId == providerId);
        
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
