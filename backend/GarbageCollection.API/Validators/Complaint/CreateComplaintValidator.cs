using FluentValidation;
using GarbageCollection.Common.DTOs.Complaint;

namespace GarbageCollection.API.Validators.Complaint
{
    public class CreateComplaintValidator : AbstractValidator<CreateComplaintDto>
    {
        public CreateComplaintValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("reason is required")
                .MaximumLength(1000).WithMessage("reason must not exceed 1000 characters");
        }
    }
}
