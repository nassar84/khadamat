using FluentValidation;

namespace Khadamat.Application.Features.Services.Commands;

public class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("اسم النشاط مطلوب.")
            .MinimumLength(3).WithMessage("اسم النشاط يجب أن يكون 3 أحرف على الأقل.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("وصف النشاط مطلوب.")
            .MinimumLength(10).WithMessage("الوصف يجب أن يكون 10 أحرف على الأقل.");

        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage("الرجاء اختيار القسم الرئيسي.");

        RuleFor(x => x.CityId)
            .NotNull().WithMessage("الرجاء اختيار المدينة.")
            .GreaterThan(0).WithMessage("الرجاء اختيار المدينة.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("العنوان بالتفصيل مطلوب.");

        RuleFor(x => x.Images)
            .Must(x => x == null || x.Count <= 10).WithMessage("لا يمكن إضافة أكثر من 10 صور.");
    }
}
