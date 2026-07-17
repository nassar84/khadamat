using MediatR;
using System.Collections.Generic;

namespace Khadamat.Application.Features.Services.Commands;

public class UpdateServiceEditRequestCommand : IRequest<bool>
{
    public int RequestId { get; set; }
    public string Status { get; set; } // Approved, Rejected, ForwardedToProvider
    public string? AdminNotes { get; set; }
    public string? ProviderNotes { get; set; }

    // Selection for partial approval
    public bool ApproveName { get; set; }
    public bool ApproveDescription { get; set; }
    public bool ApproveAddress { get; set; }
    public bool ApprovePrice { get; set; }
    public bool ApprovePhone1 { get; set; }
    public bool ApprovePhone2 { get; set; }
    public bool ApproveWhatsApp { get; set; }
}
