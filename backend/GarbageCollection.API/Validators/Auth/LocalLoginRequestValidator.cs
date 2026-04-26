using FluentValidation;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Auth;

namespace GarbageCollection.API.Validators.Auth
{
    public class LocalLoginRequestValidator : AbstractValidator<LocalLoginRequestWrapper>
    {
        public LocalLoginRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new LocalLoginDataValidator());
        }
    }

    public class LocalLoginDataValidator : AbstractValidator<LocalLoginRequestDto>
    {
        public LocalLoginDataValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required");
        }
    }
}
