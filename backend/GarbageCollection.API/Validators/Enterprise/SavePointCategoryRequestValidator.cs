using FluentValidation;
using GarbageCollection.Common.DTOs.Enterprise;
using GarbageCollection.Common.Enums;

namespace GarbageCollection.API.Validators.Enterprise
{
    public class SavePointCategoryRequestValidator : AbstractValidator<SavePointCategoryRequest>
    {
        public SavePointCategoryRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new SavePointCategoryDataValidator());
        }
    }

    public class SavePointCategoryDataValidator : AbstractValidator<SavePointCategoryData>
    {
        public SavePointCategoryDataValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("name is required")
                .MaximumLength(256).WithMessage("name must not exceed 256 characters");

            RuleFor(x => x.Mechanic)
                .IsInEnum().WithMessage("mechanic must be a valid PointMechanic value");
        }
    }
}
