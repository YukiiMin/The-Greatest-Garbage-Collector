using FluentValidation;
using GarbageCollection.Common.DTOs;

namespace GarbageCollection.API.Validators.Auth
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new ResetPasswordDataValidator());
        }
    }

    public class ResetPasswordDataValidator : AbstractValidator<ResetPasswordData>
    {
        public ResetPasswordDataValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required")
                .Matches(ValidatorConstants.EmailRegex).WithMessage("Invalid email format");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("otp is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required")
                .Matches(ValidatorConstants.PasswordRegex)
                .WithMessage("Password must be 8-16 characters and contain at least 1 uppercase, 1 lowercase, 1 digit, and 1 special character");
        }
    }
}
