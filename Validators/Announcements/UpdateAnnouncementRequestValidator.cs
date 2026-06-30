using FluentValidation;
using SmartApart.API.DTOs.Announcements;

namespace SmartApart.API.Validators.Announcements;

public class UpdateAnnouncementRequestValidator : AbstractValidator<UpdateAnnouncementRequestDto>
{
    public UpdateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.");

        RuleFor(x => x.NoticeType)
            .IsInEnum().WithMessage("Invalid notice type.");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ScheduledAt.HasValue)
            .WithMessage("Scheduled time must be in the future.");
    }
}