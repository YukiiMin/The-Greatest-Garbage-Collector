using FluentValidation;
using GarbageCollection.Common.DTOs.Collector;

namespace GarbageCollection.API.Validators.Collector
{
    public class EndShiftRequestValidator : AbstractValidator<EndShiftRequest>
    {
        public EndShiftRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new EndShiftDataValidator());
        }
    }

    public class EndShiftDataValidator : AbstractValidator<EndShiftData>
    {
        public EndShiftDataValidator()
        {
            RuleFor(x => x.TeamId)
                .NotEmpty().WithMessage("team_id is required");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("date is required");
        }
    }
}
