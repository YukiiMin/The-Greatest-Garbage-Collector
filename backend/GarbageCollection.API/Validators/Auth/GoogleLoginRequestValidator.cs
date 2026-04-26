using FluentValidation;
using GarbageCollection.Common.DTOs.Auth;

namespace GarbageCollection.API.Validators.Auth
{
    public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequestWrapper>
    {
        public GoogleLoginRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new GoogleLoginDataValidator());
        }
    }

    public class GoogleLoginDataValidator : AbstractValidator<GoogleLoginRequestDto>
    {
        public GoogleLoginDataValidator()
        {
            RuleFor(x => x.GoogleToken)
                .NotEmpty().WithMessage("google_token is required");
        }
    }
}
