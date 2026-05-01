using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Khadamat.Application.Features.Services.Commands;

public class CreateServiceEditRequestCommand : IRequest<bool>
{
    public int ServiceId { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "سبب التعديل مطلوب")]
    public string Reason { get; set; } = string.Empty;
    
    public string? ProposedName { get; set; }
    public string? ProposedDescription { get; set; }
    public string? ProposedAddress { get; set; }
    public decimal? ProposedPrice { get; set; }
    public string? ProposedPhone1 { get; set; }
    public string? ProposedPhone2 { get; set; }
    public string? ProposedWhatsApp { get; set; }
}
