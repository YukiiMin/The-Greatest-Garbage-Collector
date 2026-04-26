using FluentValidation;
using GarbageCollection.Common.DTOs;

namespace GarbageCollection.API.Validators.Auth
{
    public class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequestWrapper>
    {
        public ResendOtpRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new ResendOtpDataValidator());
        }
    }

    public class ResendOtpDataValidator : AbstractValidator<ResendOtpRequestDto>
    {
        public ResendOtpDataValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required")
                .Matches(ValidatorConstants.EmailRegex).WithMessage("Invalid email format");
        }
    }
}
