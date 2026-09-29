using FluentValidation;

namespace DueJobs.Api;

public sealed class CreateJobValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobValidator(IClock clock)
    {
        RuleFor(input => input.Title).NotEmpty().WithMessage("Title is required.");
        RuleFor(input => input.ScheduledAt)
            .Must(scheduledAt => scheduledAt > clock.UtcNow)
            .WithMessage("Scheduled time must be in the future.");
    }
}
