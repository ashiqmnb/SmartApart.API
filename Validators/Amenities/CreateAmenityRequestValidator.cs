using FluentValidation;
using SmartApart.API.DTOs.Amenities;

namespace SmartApart.API.Validators.Amenities;

public class CreateAmenityRequestValidator : AbstractValidator<CreateAmenityRequestDto>
{
    public CreateAmenityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(200);

        RuleFor(x => x.ClosingTime)
            .GreaterThan(x => x.OpeningTime)
            .WithMessage("Closing time must be after opening time.");
    }
}