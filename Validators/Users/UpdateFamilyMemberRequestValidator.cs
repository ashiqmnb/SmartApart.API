using FluentValidation;
using SmartApart.API.DTOs.Residents;

namespace SmartApart.API.Validators.Users
{
    public class UpdateFamilyMemberRequestValidator : AbstractValidator<UpdateFamilyMemberRequestDto>
    {
        public UpdateFamilyMemberRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.Relationship)
                .NotEmpty().WithMessage("Relationship is required.")
                .MaximumLength(50).WithMessage("Relationship cannot exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }

}
