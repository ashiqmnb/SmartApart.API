using FluentValidation;
using SmartApart.API.DTOs.Amenities;

namespace SmartApart.API.Validators.Amenities;

public class UpdateAmenityAvailabilityRequestValidator : AbstractValidator<UpdateAmenityAvailabilityRequestDto>
{
    public UpdateAmenityAvailabilityRequestValidator()
    {
        RuleFor(x => x.Availability)
            .IsInEnum().WithMessage("Invalid availability status.");
    }
}