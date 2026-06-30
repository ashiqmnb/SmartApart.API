using FluentValidation;
using SmartApart.API.DTOs.Maintenance;
using SmartApart.API.Enums;

namespace SmartApart.API.Validators.Maintenance
{
    public class UpdateMaintenanceStatusRequestValidator : AbstractValidator<UpdateMaintenanceStatusRequestDto>
    {
        public UpdateMaintenanceStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => Enum.TryParse<MaintenanceStatus>(s, true, out _))
                .WithMessage("Invalid status value.");
        }
    }

}
