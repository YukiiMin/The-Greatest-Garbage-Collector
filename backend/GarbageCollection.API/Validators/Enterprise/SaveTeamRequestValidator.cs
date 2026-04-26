using FluentValidation;
using GarbageCollection.Common.DTOs.Enterprise;

namespace GarbageCollection.API.Validators.Enterprise
{
    public class SaveTeamRequestValidator : AbstractValidator<SaveTeamRequest>
    {
        public SaveTeamRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new SaveTeamDataValidator());
        }
    }

    public class SaveTeamDataValidator : AbstractValidator<SaveTeamData>
    {
        public SaveTeamDataValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("name is required")
                .MaximumLength(256).WithMessage("name must not exceed 256 characters");

            RuleFor(x => x.CollectorId)
                .NotEmpty().WithMessage("collector_id is required");

            RuleFor(x => x.TotalCapacity)
                .GreaterThan(0).WithMessage("total_capacity must be greater than 0")
                .When(x => x.TotalCapacity != 0);
        }
    }
}
