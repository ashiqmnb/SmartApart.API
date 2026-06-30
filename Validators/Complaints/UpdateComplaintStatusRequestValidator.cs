using FluentValidation;
using SmartApart.API.DTOs.Complaints;
using SmartApart.API.Enums;

namespace SmartApart.API.Validators.Complaints
{
    public class UpdateComplaintStatusRequestValidator : AbstractValidator<UpdateComplaintStatusRequestDto>
    {
        public UpdateComplaintStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => Enum.TryParse<ComplaintStatus>(s, true, out _))
                .WithMessage("Invalid status value.");
        }
    }

}
