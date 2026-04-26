using FluentValidation;
using GarbageCollection.Common.DTOs.Complaint;

namespace GarbageCollection.API.Validators.Complaint
{
    public class SendComplaintMessageRequestValidator : AbstractValidator<SendComplaintMessageRequest>
    {
        public SendComplaintMessageRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new SendComplaintMessageDataValidator());
        }
    }

    public class SendComplaintMessageDataValidator : AbstractValidator<SendComplaintMessageData>
    {
        public SendComplaintMessageDataValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("message is required")
                .MaximumLength(1000).WithMessage("message must not exceed 1000 characters");
        }
    }
}
