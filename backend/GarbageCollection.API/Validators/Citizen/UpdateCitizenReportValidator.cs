using FluentValidation;
using GarbageCollection.Common.DTOs.CitizenReport;

namespace GarbageCollection.API.Validators.Citizen
{
    public class UpdateCitizenReportValidator : AbstractValidator<UpdateCitizenReportDto>
    {
        public UpdateCitizenReportValidator()
        {
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
