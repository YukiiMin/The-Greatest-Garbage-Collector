using FluentValidation;
using GarbageCollection.Common.DTOs.User;

namespace GarbageCollection.API.Validators.User
{
    public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
    {
        public UpdateUserProfileRequestValidator()
        {
            RuleFor(x => x.Fullname)
                .NotEmpty().WithMessage("fullname is required")
                .MaximumLength(256).WithMessage("fullname must not exceed 256 characters");
        }
    }
}
