using FluentValidation;
using SmartApart.API.DTOs.Residents;
using SmartApart.API.Enums;

namespace SmartApart.API.Validators.Users
{
    public class UpdateResidentRequestValidator : AbstractValidator<UpdateResidentRequestDto>
    {
        public UpdateResidentRequestValidator()
        {
            RuleFor(x => x.ApartmentNumber)
                .NotEmpty().WithMessage("Apartment number is required.")
                .MaximumLength(20).WithMessage("Apartment number cannot exceed 20 characters.");

            RuleFor(x => x.Block)
                .NotEmpty().WithMessage("Block is required.")
                .MaximumLength(10).WithMessage("Block cannot exceed 10 characters.");

            RuleFor(x => x.Floor)
                .GreaterThanOrEqualTo(0).WithMessage("Floor must be 0 or greater.");

            RuleFor(x => x.OwnershipType)
                .NotEmpty().WithMessage("Ownership type is required.")
                .Must(o => Enum.TryParse<OwnershipType>(o, out _))
                .WithMessage("Ownership type must be Owner or Renter.");

            RuleFor(x => x.MoveInDate)
                .NotEmpty().WithMessage("Move-in date is required.");

            RuleFor(x => x.EmergencyContactPhone)
                .MaximumLength(20).WithMessage("Emergency contact phone cannot exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactPhone));

            RuleFor(x => x.EmergencyContactName)
                .MaximumLength(100).WithMessage("Emergency contact name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactName));
        }
    }

}
