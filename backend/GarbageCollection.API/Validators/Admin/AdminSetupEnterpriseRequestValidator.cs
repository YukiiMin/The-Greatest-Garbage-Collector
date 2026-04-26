using FluentValidation;
using GarbageCollection.Common.DTOs.Admin;

namespace GarbageCollection.API.Validators.Admin
{
    public class AdminSetupEnterpriseRequestValidator : AbstractValidator<AdminSetupEnterpriseRequest>
    {
        public AdminSetupEnterpriseRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new SetupEnterpriseDataValidator());
        }
    }

    public class SetupEnterpriseDataValidator : AbstractValidator<SetupEnterpriseData>
    {
        public SetupEnterpriseDataValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("user_id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("name is required")
                .MaximumLength(256).WithMessage("name must not exceed 256 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("phone_number is required")
                .Matches(ValidatorConstants.PhoneRegex).WithMessage("phone_number must be 10-11 digits");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("address is required")
                .MaximumLength(512).WithMessage("address must not exceed 512 characters");

            // work_area_id is optional during setup
        }
    }
}
