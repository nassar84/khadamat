using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Khadamat.Domain.Entities;
using Khadamat.Application.Interfaces;
using Khadamat.Application.Common.Helpers;
using System;
using System.Linq;

namespace Khadamat.Application.Features.Services.Handlers;

public class UpdateServiceHandler : IRequestHandler<Commands.UpdateServiceCommand, bool>
{
    private readonly IGenericRepository<Service> _serviceRepository;
    private readonly IGenericRepository<ProviderProfile> _providerRepository;
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IGenericRepository<SubCategory> _subCategoryRepository;
    private readonly IGenericRepository<ServiceImage> _imageRepository;

    public UpdateServiceHandler(
        IGenericRepository<Service> serviceRepository,
        IGenericRepository<ProviderProfile> providerRepository,
        IGenericRepository<Category> categoryRepository,
        IGenericRepository<SubCategory> subCategoryRepository,
        IGenericRepository<ServiceImage> imageRepository)
    {
        _serviceRepository = serviceRepository;
        _providerRepository = providerRepository;
        _categoryRepository = categoryRepository;
        _subCategoryRepository = subCategoryRepository;
        _imageRepository = imageRepository;
    }

    public async Task<bool> Handle(Commands.UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.Id);
        
        if (service == null)
        {
            return false;
        }

        // Verify ownership
        var provider = await _providerRepository.GetAsync(p => p.UserId == request.UserId);
        if (provider == null || service.ProviderProfileId != provider.Id)
        {
            throw new UnauthorizedAccessException("You do not have permission to edit this service.");
        }

        int? catId = request.CategoryId ?? service.CategoryId;
        int? subCatId = request.SubCategoryId ?? service.SubCategoryId;
        bool isClinic = await IsClinicCategoryAsync(catId, subCatId);
        string formattedTitle = ClinicServiceHelper.FormatDoctorNameIfNeeded(request.Title, isClinic);

        // Update basic details using Domain methods
        service.UpdateDetails(
            formattedTitle,
            request.Description,
            request.Address ?? string.Empty,
            request.Price,
            request.Phone1,
            request.Phone2,
            request.WhatsApp,
            request.Facebook,
            request.Telegram,
            request.WorkDays,
            request.WorkHours,
            request.Specialization,
            request.SpecializationId
        );

        // Update hierarchy using Domain methods
        service.SetCategory(request.CategoryId, request.SubCategoryId);
        
        // Update location
        service.UpdateLocation(request.CityId);
        
        // Update images
        if (request.Images != null)
        {
            if (request.Images.Any())
            {
                service.SetImage(request.Images.First());
            }
            else
            {
                service.SetImage(null);
            }

            // Sync ServiceImage collection
            try
            {
                var existingImages = await _imageRepository.GetPagedAsync(1, 100, filter: img => img.ServiceId == service.Id);
                foreach (var img in existingImages)
                {
                    await _imageRepository.DeleteAsync(img);
                }

                int order = 0;
                foreach (var imgUrl in request.Images.Distinct())
                {
                    if (!string.IsNullOrWhiteSpace(imgUrl))
                    {
                        await _imageRepository.AddAsync(new ServiceImage(service.Id, imgUrl, order, order == 0));
                        order++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateServiceHandler] Error syncing service images: {ex.Message}");
            }
        }

        await _serviceRepository.UpdateAsync(service);
        
        return true;
    }

    private async Task<bool> IsClinicCategoryAsync(int? categoryId, int? subCategoryId)
    {
        try
        {
            if (categoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId.Value);
                if (category != null && category.Name.Trim().Contains("عيادات"))
                    return true;
            }

            if (subCategoryId.HasValue)
            {
                var subCat = await _subCategoryRepository.GetByIdAsync(subCategoryId.Value, includeProperties: "Category");
                if (subCat != null)
                {
                    if (subCat.Category != null && subCat.Category.Name.Trim().Contains("عيادات"))
                        return true;

                    if (subCat.CategoryId > 0)
                    {
                        var parentCat = await _categoryRepository.GetByIdAsync(subCat.CategoryId);
                        if (parentCat != null && parentCat.Name.Trim().Contains("عيادات"))
                            return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateServiceHandler] IsClinicCategoryAsync check failed: {ex.Message}");
        }

        return false;
    }
}
