using System.Threading;
using System.Threading.Tasks;
using Khadamat.Domain.Entities;
using Khadamat.Application.Interfaces;
using Khadamat.Application.Common.Helpers;
using MediatR;
using Khadamat.Application.Features.Services.Commands;
using System;

namespace Khadamat.Application.Features.Services.Handlers;

public class UpdateServiceEditRequestHandler : IRequestHandler<UpdateServiceEditRequestCommand, bool>
{
    private readonly IGenericRepository<Service> _serviceRepo;
    private readonly IGenericRepository<ServiceEditRequest> _requestRepo;
    private readonly IGenericRepository<Notification> _notifRepo;
    private readonly IGenericRepository<Category> _categoryRepo;
    private readonly IGenericRepository<SubCategory> _subCategoryRepo;

    public UpdateServiceEditRequestHandler(
        IGenericRepository<Service> serviceRepo, 
        IGenericRepository<ServiceEditRequest> requestRepo,
        IGenericRepository<Notification> notifRepo,
        IGenericRepository<Category> categoryRepo,
        IGenericRepository<SubCategory> subCategoryRepo)
    {
        _serviceRepo = serviceRepo;
        _requestRepo = requestRepo;
        _notifRepo = notifRepo;
        _categoryRepo = categoryRepo;
        _subCategoryRepo = subCategoryRepo;
    }

    public async Task<bool> Handle(UpdateServiceEditRequestCommand request, CancellationToken cancellationToken)
    {
        var editRequest = await _requestRepo.GetByIdAsync(request.RequestId, includeProperties: "Service");
        if (editRequest == null || editRequest.Service == null) return false;

        editRequest.Status = request.Status;
        editRequest.AdminNotes = request.AdminNotes;

        if (request.Status == "Approved")
        {
            var service = editRequest.Service;

            // Prepare values (mix of current and proposed)
            string name = request.ApproveName && !string.IsNullOrEmpty(editRequest.ProposedName) ? editRequest.ProposedName : service.Name;
            bool isClinic = await IsClinicCategoryAsync(service.CategoryId, service.SubCategoryId);
            name = ClinicServiceHelper.FormatDoctorNameIfNeeded(name, isClinic);

            string desc = request.ApproveDescription && !string.IsNullOrEmpty(editRequest.ProposedDescription) ? editRequest.ProposedDescription : service.Description;
            string addr = request.ApproveAddress && !string.IsNullOrEmpty(editRequest.ProposedAddress) ? editRequest.ProposedAddress : service.Address;
            decimal? price = request.ApprovePrice && editRequest.ProposedPrice.HasValue ? editRequest.ProposedPrice : service.Price;
            string? phone1 = request.ApprovePhone1 && !string.IsNullOrEmpty(editRequest.ProposedPhone1) ? editRequest.ProposedPhone1 : service.Phone1;
            string? phone2 = request.ApprovePhone2 && !string.IsNullOrEmpty(editRequest.ProposedPhone2) ? editRequest.ProposedPhone2 : service.Phone2;
            string? whatsApp = request.ApproveWhatsApp && !string.IsNullOrEmpty(editRequest.ProposedWhatsApp) ? editRequest.ProposedWhatsApp : service.WhatsApp;

            service.UpdateDetails(
                name, 
                desc, 
                addr, 
                price, 
                phone1, 
                phone2, 
                whatsApp, 
                service.Facebook, 
                service.Telegram, 
                service.Work_Days, 
                service.Work_Houers
            );

            // Update flags in the request record for history
            editRequest.ApprovedName = request.ApproveName;
            editRequest.ApprovedDescription = request.ApproveDescription;
            editRequest.ApprovedAddress = request.ApproveAddress;
            editRequest.ApprovedPrice = request.ApprovePrice;
            editRequest.ApprovedPhone1 = request.ApprovePhone1;

            await _serviceRepo.UpdateAsync(service);
        }
        else if (request.Status == "ForwardedToProvider")
        {
            // Logic to notify the provider
            if (!string.IsNullOrEmpty(editRequest.Service.UserCreated))
            {
                 // Create Notification for the provider
                 var notification = new Notification(
                     editRequest.Service.UserCreated,
                     "اقتراح تعديل على خدمتك",
                     $"هناك اقتراح لتعديل بيانات خدمتك '{editRequest.Service.Name}' من قبل أحد المستخدمين. يرجى مراجعته.",
                     "EditRequest",
                     editRequest.Id.ToString()
                 );
                 await _notifRepo.AddAsync(notification);
            }
        }

        await _requestRepo.UpdateAsync(editRequest);
        return true;
    }

    private async Task<bool> IsClinicCategoryAsync(int? categoryId, int? subCategoryId)
    {
        try
        {
            if (categoryId.HasValue)
            {
                var category = await _categoryRepo.GetByIdAsync(categoryId.Value);
                if (category != null && category.Name.Trim().Contains("عيادات"))
                    return true;
            }

            if (subCategoryId.HasValue)
            {
                var subCat = await _subCategoryRepo.GetByIdAsync(subCategoryId.Value, includeProperties: "Category");
                if (subCat != null)
                {
                    if (subCat.Category != null && subCat.Category.Name.Trim().Contains("عيادات"))
                        return true;

                    if (subCat.CategoryId > 0)
                    {
                        var parentCat = await _categoryRepo.GetByIdAsync(subCat.CategoryId);
                        if (parentCat != null && parentCat.Name.Trim().Contains("عيادات"))
                            return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateServiceEditRequestHandler] IsClinicCategoryAsync check failed: {ex.Message}");
        }

        return false;
    }
}
