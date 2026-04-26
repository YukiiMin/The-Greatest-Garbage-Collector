using FluentValidation;
using GarbageCollection.Common.DTOs.Complaint;

namespace GarbageCollection.API.Validators.Complaint
{
    public class ResolveComplaintRequestValidator : AbstractValidator<ResolveComplaintRequest>
    {
        private static readonly IReadOnlySet<string> ValidStatuses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "APPROVED", "REJECTED" };

        public ResolveComplaintRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new ResolveComplaintDataValidator());
        }
    }

    public class ResolveComplaintDataValidator : AbstractValidator<ResolveComplaintData>
    {
        private static readonly IReadOnlySet<string> ValidStatuses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "APPROVED", "REJECTED" };

        public ResolveComplaintDataValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("status is required")
                .Must(s => ValidStatuses.Contains(s))
                .WithMessage("status must be APPROVED or REJECTED");

            RuleFor(x => x.AdminResponse)
                .NotEmpty().WithMessage("admin_response is required")
                .MaximumLength(2000).WithMessage("admin_response must not exceed 2000 characters");
        }
    }
}
