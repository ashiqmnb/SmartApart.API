using FluentValidation;
using SmartApart.API.DTOs.Notifications;

namespace SmartApart.API.Validators.Notifications
{
    public class DeregisterFcmTokenRequestValidator : AbstractValidator<DeregisterFcmTokenRequestDto>
    {
        public DeregisterFcmTokenRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("FCM token is required.");
        }
    }

}
