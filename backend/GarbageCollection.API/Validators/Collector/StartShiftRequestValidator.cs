using FluentValidation;
using GarbageCollection.Common.DTOs.Collector;

namespace GarbageCollection.API.Validators.Collector
{
    public class StartShiftRequestValidator : AbstractValidator<StartShiftRequest>
    {
        public StartShiftRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new StartShiftDataValidator());
        }
    }

    public class StartShiftDataValidator : AbstractValidator<StartShiftData>
    {
        public StartShiftDataValidator()
        {
            RuleFor(x => x.TeamId)
                .NotEmpty().WithMessage("team_id is required");

            RuleFor(x => x.Date)
                .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("date must not be in the future");
        }
    }
}
