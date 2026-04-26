using FluentValidation;
using GarbageCollection.Common.DTOs.User;

namespace GarbageCollection.API.Validators.User
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new ChangePasswordDataValidator());
        }
    }

    public class ChangePasswordDataValidator : AbstractValidator<ChangePasswordData>
    {
        public ChangePasswordDataValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("old_password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("new_password is required")
                .Matches(ValidatorConstants.PasswordRegex)
                .WithMessage("Password must be 8-16 characters and contain at least 1 uppercase, 1 lowercase, 1 digit, and 1 special character");
        }
    }
}
