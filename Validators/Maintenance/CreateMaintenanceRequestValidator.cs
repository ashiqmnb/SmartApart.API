using FluentValidation;
using SmartApart.API.DTOs.Maintenance;

namespace SmartApart.API.Validators.Maintenance
{
    public class CreateMaintenanceRequestValidator : AbstractValidator<CreateMaintenanceRequestDto>
    {
        public CreateMaintenanceRequestValidator()
        {
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid maintenance category.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid priority level.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");
        }
    }

}
