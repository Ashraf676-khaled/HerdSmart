// Application/Features/Telemetry/Commands/IngestTelemetryReading/IngestTelemetryReadingCommandValidator.cs
using FluentValidation;
using HerdSmart.Domain.Enums;

namespace Application.Features.Telemetry.Commands.IngestTelemetryReading;

public class IngestTelemetryReadingCommandValidator : AbstractValidator<IngestTelemetryReadingCommand>
{
    public IngestTelemetryReadingCommandValidator()
    {
        RuleFor(x => x.CattleId)
            .NotEmpty();

        RuleFor(x => x.SensorType)
            .IsInEnum();

        RuleFor(x => x.RecordedAt)
            .NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(5))
                .WithMessage("Recorded date cannot be in the future.");

        RuleFor(x => x.Value)
            .NotEmpty();

        When(x => x.SensorType == SensorType.Temperature, () =>
        {
            RuleFor(x => x.Value)
                .InclusiveBetween(30.0, 45.0)
                    .WithMessage("Temperature value must be between 30°C and 45°C.");
        });

        When(x => x.SensorType == SensorType.ActivityLevel, () =>
        {
            RuleFor(x => x.Value)
                .InclusiveBetween(0.0, 100.0)
                    .WithMessage("Activity level must be between 0 and 100.");
        });
    }
}