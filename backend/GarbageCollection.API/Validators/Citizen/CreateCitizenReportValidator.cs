using FluentValidation;
using GarbageCollection.Common.DTOs.CitizenReport;

namespace GarbageCollection.API.Validators.Citizen
{
    public class CreateCitizenReportValidator : AbstractValidator<CreateCitizenReportDto>
    {
        public CreateCitizenReportValidator()
        {
            RuleFor(x => x.Types)
                .NotNull().WithMessage("types is required")
                .Must(t => t != null && t.Count > 0).WithMessage("At least one waste type is required");

            RuleFor(x => x.Capacity)
                .InclusiveBetween(0.01m, 10000m)
                .WithMessage("Capacity must be between 0.01 and 10000 kg")
                .When(x => x.Capacity.HasValue);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
                .When(x => x.Description is not null);
        }
    }
}
