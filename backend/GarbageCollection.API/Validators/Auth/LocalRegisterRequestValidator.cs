using FluentValidation;
using GarbageCollection.Common.DTOs.Auth.Local;

namespace GarbageCollection.API.Validators.Auth
{
    public class LocalRegisterRequestValidator : AbstractValidator<LocalRegisterRequestWrapper>
    {
        public LocalRegisterRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new LocalRegisterRequestDataValidator());
        }
    }

    public class LocalRegisterRequestDataValidator : AbstractValidator<LocalRegisterRequestDto>
    {
        public LocalRegisterRequestDataValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required")
                .Matches(ValidatorConstants.EmailRegex).WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required")
                .Matches(ValidatorConstants.PasswordRegex)
                .WithMessage("Password must be 8-16 characters and contain at least 1 uppercase, 1 lowercase, 1 digit, and 1 special character");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("full_name is required")
                .MaximumLength(256).WithMessage("full_name must not exceed 256 characters");
        }
    }
}
