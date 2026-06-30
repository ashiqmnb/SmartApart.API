using FluentValidation;
using SmartApart.API.DTOs.Maintenance;

namespace SmartApart.API.Validators.Maintenance
{
    public class AssignMaintenanceRequestValidator : AbstractValidator<AssignMaintenanceRequestDto>
    {
        public AssignMaintenanceRequestValidator()
        {
            RuleFor(x => x.AssignedTo)
                .NotEmpty().WithMessage("Assigned staff ID is required.");
        }
    }

}
