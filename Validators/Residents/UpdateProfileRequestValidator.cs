using FluentValidation;
using SmartApart.API.DTOs.Users;

namespace SmartApart.API.Validators.Residents
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequestDto>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");
        }
    }
}
