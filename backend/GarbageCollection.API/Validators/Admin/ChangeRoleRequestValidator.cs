using FluentValidation;
using GarbageCollection.Common.DTOs.Admin;
using GarbageCollection.Common.Enums;

namespace GarbageCollection.API.Validators.Admin
{
    public class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
    {
        private static readonly IReadOnlySet<string> ValidRoles =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "Citizen", "Collector", "Enterprise", "Admin" };

        public ChangeRoleRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new ChangeRoleDataValidator());
        }
    }

    public class ChangeRoleDataValidator : AbstractValidator<ChangeRoleData>
    {
        private static readonly IReadOnlySet<string> ValidRoles =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "Citizen", "Collector", "Enterprise", "Admin" };

        public ChangeRoleDataValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("role is required")
                .Must(r => ValidRoles.Contains(r))
                .WithMessage("role must be one of: Citizen, Collector, Enterprise, Admin");
        }
    }
}
