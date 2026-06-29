using FluentValidation;
using SmartApart.API.DTOs.Auth;

namespace SmartApart.API.Validators.Auth
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required.")
                .Length(6).WithMessage("OTP must be 6 digits.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
