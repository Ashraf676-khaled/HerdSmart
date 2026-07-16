// Application/DependencyInjection.cs
using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Features.Telemetry.Jobs;
using FluentValidation;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // 6. Background Jobs
        services.AddScoped<MarkOverdueVaccinationsJob>();
        services.AddScoped<AutoGenerateVaccinationSchedulesJob>();
        services.AddScoped<CleanupResolvedTelemetryAlertsJob>();

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}