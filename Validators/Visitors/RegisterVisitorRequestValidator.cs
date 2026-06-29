using FluentValidation;
using SmartApart.API.DTOs.Visitors;

namespace SmartApart.API.Validators.Visitors
{
    public class RegisterVisitorRequestValidator : AbstractValidator<RegisterVisitorRequestDto>
    {
        public RegisterVisitorRequestValidator()
        {
            RuleFor(x => x.ResidentId)
                .NotEmpty().WithMessage("Resident ID is required.");

            RuleFor(x => x.VisitorName)
                .NotEmpty().WithMessage("Visitor name is required.")
                .MaximumLength(100).WithMessage("Visitor name cannot exceed 100 characters.");

            RuleFor(x => x.VisitorPhone)
                .NotEmpty().WithMessage("Visitor phone is required.")
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");

            RuleFor(x => x.Purpose)
                .NotEmpty().WithMessage("Purpose of visit is required.");

            RuleFor(x => x.VehicleNumber)
                .MaximumLength(20).WithMessage("Vehicle number cannot exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.VehicleNumber));
        }
    }
}
