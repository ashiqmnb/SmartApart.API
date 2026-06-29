using FluentValidation;
using SmartApart.API.DTOs.Users;

namespace SmartApart.API.Validators.Users
{
    public class AdminCreateUserRequestValidator : AbstractValidator<AdminCreateUserRequestDto>
    {
        public AdminCreateUserRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(r => new[] { "Resident", "Security" }.Contains(r))
                .WithMessage("Admin can only create Resident or Security users.");
        }
    }
}
