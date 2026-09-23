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
    private readonly IGenericRepository<ProviderProfile> _providerRepository;
    private readonly IAuthService _authService;
    private readonly IGenericRepository<SubscriptionPlan> _planRepository;
    private readonly IGenericRepository<ProviderSubscription> _subRepository;

    public CreateServiceHandler(
        IGenericRepository<Service> serviceRepository,
        IGenericRepository<ProviderProfile> providerRepository,
        IAuthService authService,
        IGenericRepository<SubscriptionPlan> planRepository,
        IGenericRepository<ProviderSubscription> subRepository)
    {
        _serviceRepository = serviceRepository;
        _providerRepository = providerRepository;
        _authService = authService;
        _planRepository = planRepository;
        _subRepository = subRepository;
    }

    public async Task<int> Handle(Commands.CreateServiceCommand request, CancellationToken cancellationToken)
    {
        // 1. Ensure Provider Profile exists (Required by FK)
        var provider = await _providerRepository.GetAsync(p => p.UserId == request.UserId);
        
        if (provider == null)
        {
            // Create a new Provider Profile explicitly
            provider = new ProviderProfile 
            {
                UserId = request.UserId,
                BusinessName = request.Title,
                Bio = !string.IsNullOrEmpty(request.Description) && request.Description.Length > 200 
                      ? request.Description.Substring(0, 200) 
                      : request.Description,
                ContactNumber = request.Phone1 ?? string.Empty,
                CityId = request.CityId,
                Location = request.Address ?? string.Empty,
                Verified = false // Pending approval
            };
            
            // Set image if available
            if (request.Images != null && request.Images.Any())
            {
                provider.Photo = request.Images.First();
            }

            await _providerRepository.AddAsync(provider);
            
            // Mark user as provider in Identity System
            await _authService.SetUserIsProviderAsync(request.UserId, true);
        }

        // 1.1 Ensure Provider has an active Subscription (Free Plan - 90 days default)
        if (provider.SubscriptionId == null)
        {
            try
            {
                var freePlans = await _planRepository.GetPagedAsync(1, 1, filter: p => p.Price == 0);
                var freePlan = freePlans.FirstOrDefault() ?? (await _planRepository.GetPagedAsync(1, 1)).FirstOrDefault();

                if (freePlan != null)
                {
                    int duration = freePlan.DurationInDays > 0 ? freePlan.DurationInDays : 90;
                    var newSub = new ProviderSubscription(provider.Id, freePlan.Id, duration);
                    await _subRepository.AddAsync(newSub);
                    provider.SubscriptionId = newSub.Id;
                    await _providerRepository.UpdateAsync(provider);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateServiceHandler] Could not auto-assign free subscription: {ex.Message}");
            }
        }

        if (request.CategoryId.HasValue == false && request.SubCategoryId.HasValue == false)
            throw new Exception("Service must belong to a Category or SubCategory");

        // 2. Create Service Entity using Rich Domain Constructor
        var service = new Service(
            request.SubCategoryId,
            request.CategoryId,
            request.CityId,
            request.Title,
            request.Description,
            request.Location ?? string.Empty,
            provider.Id, // ProviderProfileId
            request.UserId // UserCreated
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
