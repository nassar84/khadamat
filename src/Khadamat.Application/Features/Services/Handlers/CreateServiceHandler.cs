using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Khadamat.Domain.Entities;
using Khadamat.Application.Interfaces;
using System;
using System.Linq;

namespace Khadamat.Application.Features.Services.Handlers;

public class CreateServiceHandler : IRequestHandler<Commands.CreateServiceCommand, int>
{
    private readonly IGenericRepository<Service> _serviceRepository;

    public CreateServiceHandler(IGenericRepository<Service> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<int> Handle(Commands.CreateServiceCommand request, CancellationToken cancellationToken)
    {
        // Any authenticated user can create a service
        // The UI should ensure profile is complete before allowing service creation
        
        // Create Entity using Rich Domain Constructor
        var service = new Service(
            request.SubCategoryId,
            request.CategoryId,
            request.CityId,
            request.Title,
            request.Description,
            request.Location ?? string.Empty,
            request.UserId
        );

        // Update additional details
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

        // Set main image if provided
        if (request.Images != null && request.Images.Any())
        {
            service.SetImage(request.Images.First());
        }

        // Persist
        await _serviceRepository.AddAsync(service);
        
        return service.Id;
    }
}
