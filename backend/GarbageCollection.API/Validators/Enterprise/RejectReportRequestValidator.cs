using FluentValidation;
using GarbageCollection.Common.DTOs.Enterprise;

namespace GarbageCollection.API.Validators.Enterprise
{
    public class RejectReportRequestValidator : AbstractValidator<RejectReportRequest>
    {
        public RejectReportRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new RejectReportDataValidator());
        }
    }

    public class RejectReportDataValidator : AbstractValidator<RejectReportData>
    {
        public RejectReportDataValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("reason is required")
                .MaximumLength(1000).WithMessage("reason must not exceed 1000 characters");
        }
    }
}
