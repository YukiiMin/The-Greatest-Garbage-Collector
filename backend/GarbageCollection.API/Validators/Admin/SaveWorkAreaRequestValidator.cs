using FluentValidation;
using GarbageCollection.Common.DTOs.Admin;

namespace GarbageCollection.API.Validators.Admin
{
    public class SaveWorkAreaRequestValidator : AbstractValidator<SaveWorkAreaRequest>
    {
        public SaveWorkAreaRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required");

            When(x => x.Data != null, () =>
            {
                RuleFor(x => x.Data.Name)
                    .NotEmpty().WithMessage("name is required")
                    .MaximumLength(256).WithMessage("name must not exceed 256 characters");

                RuleFor(x => x.Data.Type)
                    .NotEmpty().WithMessage("type is required")
                    .Must(t => t == "District" || t == "Ward")
                    .WithMessage("type must be 'District' or 'Ward'");

                RuleFor(x => x.Data.ParentId)
                    .NotNull().WithMessage("parent_id is required for Ward")
                    .When(x => x.Data.Type == "Ward");
            });
        }
    }
}
