using FluentValidation;
using SmartApart.API.DTOs.Notifications;
using SmartApart.API.Enums;

namespace SmartApart.API.Validators.Notifications
{
    public class RegisterFcmTokenRequestValidator : AbstractValidator<RegisterFcmTokenRequestDto>
    {
        public RegisterFcmTokenRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("FCM token is required.");

            RuleFor(x => x.DeviceType)
                .NotEmpty().WithMessage("Device type is required.")
                .Must(d => Enum.TryParse<DeviceType>(d, out _))
                .WithMessage("Device type must be Android or iOS.");
        }
    }

}
