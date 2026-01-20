using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Khadamat.Domain.Entities;
using Khadamat.Application.Interfaces;
using System;
using System.Linq;

namespace Khadamat.Application.Features.Services.Handlers;

public class UpdateServiceHandler : IRequestHandler<Commands.UpdateServiceCommand, bool>
{
    private readonly IGenericRepository<Service> _serviceRepository;

    public UpdateServiceHandler(IGenericRepository<Service> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<bool> Handle(Commands.UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.Id);
        
        if (service == null)
        {
            return false;
        }

        // Verify ownership (simplified, assumes UserId matches)
        if (service.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to edit this service.");
        }

        // Update basic details using Domain methods
        service.UpdateDetails(
            request.Title,
            request.Description,
            request.Address ?? string.Empty,
            request.Price,
            request.Phone1,
            request.Phone2,
            request.WhatsApp,
            request.Facebook,
            request.Telegram,
            request.WorkDays,
            request.WorkHours
        );

        // Update hierarchy using Domain methods
        service.SetCategory(request.CategoryId, request.SubCategoryId);
        
        // Update location
        service.UpdateLocation(request.CityId);
        
        // service.YouTubeUrl = request.YouTubeUrl; // Missing in entity

        // Update images (simplified: replace with new list)
        if (request.Images != null && request.Images.Any())
        {
            service.SetImage(request.Images.First());
        }

        await _serviceRepository.UpdateAsync(service);
        
        return true;
    }
}
