using FluentValidation;
using GarbageCollection.Common.DTOs.Enterprise;

namespace GarbageCollection.API.Validators.Enterprise
{
    public class AssignReportRequestValidator : AbstractValidator<AssignReportRequest>
    {
        public AssignReportRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new AssignReportDataValidator());
        }
    }

    public class AssignReportDataValidator : AbstractValidator<AssignReportData>
    {
        public AssignReportDataValidator()
        {
            RuleFor(x => x.TeamId)
                .NotEmpty().WithMessage("team_id is required");

            RuleFor(x => x.Deadline)
                .Must(d => d > DateTime.UtcNow)
                .WithMessage("deadline must be in the future");
        }
    }
}
