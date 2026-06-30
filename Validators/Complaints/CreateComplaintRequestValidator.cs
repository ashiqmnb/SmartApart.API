using FluentValidation;
using SmartApart.API.DTOs.Complaints;

namespace SmartApart.API.Validators.Complaints
{
    public class CreateComplaintRequestValidator : AbstractValidator<CreateComplaintRequestDto>
    {
        public CreateComplaintRequestValidator()
        {
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid complaint category.");

            RuleFor(x => x.ComplaintType)
                .IsInEnum().WithMessage("Invalid complaint type.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");
        }
    }

}
