using System.Threading;
using System.Threading.Tasks;
using Khadamat.Domain.Entities;
using Khadamat.Application.Interfaces;
using MediatR;

namespace Khadamat.Application.Features.Services.Handlers;

public class CreateServiceEditRequestHandler : IRequestHandler<Commands.CreateServiceEditRequestCommand, bool>
{
    private readonly IGenericRepository<Service> _serviceRepo;
    private readonly IGenericRepository<ServiceEditRequest> _requestRepo;

    public CreateServiceEditRequestHandler(IGenericRepository<Service> serviceRepo, IGenericRepository<ServiceEditRequest> requestRepo)
    {
        _serviceRepo = serviceRepo;
        _requestRepo = requestRepo;
    }

    public async Task<bool> Handle(Commands.CreateServiceEditRequestCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepo.GetByIdAsync(request.ServiceId);
        if (service == null) return false;

        var editRequest = new ServiceEditRequest
        {
            ServiceId = request.ServiceId,
            RequesterId = request.RequesterId,
            Reason = request.Reason,
            ProposedName = request.ProposedName,
            ProposedDescription = request.ProposedDescription,
            ProposedAddress = request.ProposedAddress,
            ProposedPrice = request.ProposedPrice,
            ProposedPhone1 = request.ProposedPhone1,
            Status = "Pending"
        };

        await _requestRepo.AddAsync(editRequest);
        return true;
    }
}
