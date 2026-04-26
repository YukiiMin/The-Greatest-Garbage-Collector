using FluentValidation;
using GarbageCollection.Common.DTOs;

namespace GarbageCollection.API.Validators.Auth
{
    public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequestWrapper>
    {
        public VerifyEmailRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new VerifyEmailDataValidator());
        }
    }

    public class VerifyEmailDataValidator : AbstractValidator<VerifyEmailRequestDto>
    {
        public VerifyEmailDataValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required")
                .Matches(ValidatorConstants.EmailRegex).WithMessage("Invalid email format");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("otp is required")
                .Matches(ValidatorConstants.OtpRegex).WithMessage("OTP must be 6 digits");
        }
    }
}
