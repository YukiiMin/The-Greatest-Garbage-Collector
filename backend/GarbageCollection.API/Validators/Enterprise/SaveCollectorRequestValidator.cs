using FluentValidation;
using GarbageCollection.Common.DTOs.Enterprise;

namespace GarbageCollection.API.Validators.Enterprise
{
    public class SaveCollectorRequestValidator : AbstractValidator<SaveCollectorRequest>
    {
        public SaveCollectorRequestValidator()
        {
            RuleFor(x => x.Data).NotNull().WithMessage("data is required")
                .SetValidator(new SaveCollectorDataValidator());
        }
    }

    public class SaveCollectorDataValidator : AbstractValidator<SaveCollectorData>
    {
        public SaveCollectorDataValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("name is required")
                .MaximumLength(256).WithMessage("name must not exceed 256 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("phone_number is required")
                .Matches(ValidatorConstants.PhoneRegex).WithMessage("phone_number must be 10-11 digits");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required")
                .Matches(ValidatorConstants.EmailRegex).WithMessage("Invalid email format");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("address is required")
                .MaximumLength(512).WithMessage("address must not exceed 512 characters");

            // work_area_id is optional — validation done in service layer

            RuleFor(x => x.AssignedCapacity)
                .GreaterThan(0).WithMessage("assigned_capacity must be greater than 0")
                .When(x => x.AssignedCapacity.HasValue);

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90m, 90m).WithMessage("latitude must be between -90 and 90")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180m, 180m).WithMessage("longitude must be between -180 and 180")
                .When(x => x.Longitude.HasValue);
        }
    }
}
