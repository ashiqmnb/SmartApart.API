using FluentValidation;
using SmartApart.API.DTOs.Auth;

namespace SmartApart.API.Validators.Auth
{
    public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequestDto>
    {
        public VerifyOtpRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required.")
                .Length(6).WithMessage("OTP must be 6 digits.");

            RuleFor(x => x.Purpose)
                .NotEmpty().WithMessage("Purpose is required.")
                .Must(p => new[] { "Registration", "ForgotPassword", "ChangePhone" }.Contains(p))
                .WithMessage("Invalid OTP purpose.");
        }
    }
}
