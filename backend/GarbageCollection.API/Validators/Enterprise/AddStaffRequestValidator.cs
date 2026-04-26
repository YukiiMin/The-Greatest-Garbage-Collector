using FluentValidation;
using GarbageCollection.Common.DTOs.Enterprise;

namespace GarbageCollection.API.Validators.Enterprise
{
    public class AddStaffRequestValidator : AbstractValidator<AddStaffRequest>
    {
        public AddStaffRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("user_id is required");
        }
    }
}
