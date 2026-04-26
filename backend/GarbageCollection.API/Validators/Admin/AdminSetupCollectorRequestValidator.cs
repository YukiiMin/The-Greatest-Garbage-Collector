    using FluentValidation;
using GarbageCollection.Common.DTOs.Admin;

namespace GarbageCollection.API.Validators.Admin
{
    public class AdminSetupCollectorRequestValidator : AbstractValidator<AdminSetupCollectorRequest>
    {
        public AdminSetupCollectorRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new SetupCollectorDataValidator());
        }
    }

    public class SetupCollectorDataValidator : AbstractValidator<SetupCollectorData>
    {
        public SetupCollectorDataValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("user_id is required");

            RuleFor(x => x.EnterpriseId)
                .NotEmpty().WithMessage("enterprise_id is required");
        }
    }
}
