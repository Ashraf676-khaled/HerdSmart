// HerdSmart.Tests/Features/Telemetry/CleanupResolvedTelemetryAlertsJobTests.cs
using Application.Common.Interfaces;
using Application.Features.Telemetry.Jobs;
using HerdSmart.Domain.Entities;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;

namespace HerdSmart.Tests.Features.Telemetry;

public class CleanupResolvedTelemetryAlertsJobTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<CleanupResolvedTelemetryAlertsJob>> _loggerMock;
    private readonly CleanupResolvedTelemetryAlertsJob _job;

    public CleanupResolvedTelemetryAlertsJobTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<CleanupResolvedTelemetryAlertsJob>>();

        _job = new CleanupResolvedTelemetryAlertsJob(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoResolvedAlertsOlderThanCutoff_ShouldNotDeleteAnything()
    {
        // Arrange
        var alerts = new List<TelemetryAlert>
        {
            new() { IsResolved = false }, // لسه مفتوحة
            new() { IsResolved = true, ResolvedAt = DateTimeOffset.UtcNow.AddDays(-10) } // قريبة، مش قديمة كفاية
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.TelemetryAlerts).Returns(alerts.Object);

        // Act
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert
        alerts.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TelemetryAlert>>()), Times.Never);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenResolvedAlertsOlderThanCutoff_ShouldDeleteOnlyThose()
    {
        // Arrange
        var oldResolvedAlert = new TelemetryAlert
        {
            IsResolved = true,
            ResolvedAt = DateTimeOffset.UtcNow.AddMonths(-4)
        };

        var alerts = new List<TelemetryAlert>
        {
            oldResolvedAlert,
            new() { IsResolved = false }, // مفتوحة، متتمسحش
            new() { IsResolved = true, ResolvedAt = DateTimeOffset.UtcNow.AddDays(-5) } // قريبة، متتمسحش
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.TelemetryAlerts).Returns(alerts.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert
        alerts.Verify(
            x => x.RemoveRange(It.Is<IEnumerable<TelemetryAlert>>(
                list => list.Count() == 1 && list.Contains(oldResolvedAlert))),
            Times.Once);

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenResolvedButNoResolvedAtDate_ShouldNotDelete()
    {
        // Arrange — اتحلت بس التاريخ null، محتاجين نتأكد إنها متتمسحش غلط
        var alerts = new List<TelemetryAlert>
        {
            new() { IsResolved = true, ResolvedAt = null }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.TelemetryAlerts).Returns(alerts.Object);

        // Act
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert
        alerts.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TelemetryAlert>>()), Times.Never);
    }
}