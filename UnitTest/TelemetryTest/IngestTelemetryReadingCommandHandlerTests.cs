// HerdSmart.Tests/Features/Telemetry/IngestTelemetryReadingCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Telemetry.Commands.IngestTelemetryReading;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.Telemetry;

public class IngestTelemetryReadingCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IRealtimeNotifier> _notifierMock;
    private readonly Mock<ILogger<IngestTelemetryReadingCommandHandler>> _loggerMock;
    private readonly IngestTelemetryReadingCommandHandler _handler;

    public IngestTelemetryReadingCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _notifierMock = new Mock<IRealtimeNotifier>();
        _loggerMock = new Mock<ILogger<IngestTelemetryReadingCommandHandler>>();

        _handler = new IngestTelemetryReadingCommandHandler(
            _contextMock.Object,
            _notifierMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCattleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new IngestTelemetryReadingCommand(
            Ulid.NewUlid(), SensorType.Temperature, 39.0, DateTimeOffset.UtcNow);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValueNormal_ShouldNotCreateAlertOrNotify()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new IngestTelemetryReadingCommand(
            cattleId, SensorType.Temperature, 38.5, DateTimeOffset.UtcNow);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = Ulid.NewUlid(), TagNumber = "COW-001" }
        }.BuildMockDbSet();

        var alerts = new List<TelemetryAlert>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.TelemetryAlerts).Returns(alerts.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AlertCreated.ShouldBeFalse();
        result.AlertId.ShouldBeNull();

        alerts.Verify(x => x.Add(It.IsAny<TelemetryAlert>()), Times.Never);
        _notifierMock.Verify(
            x => x.NotifyTenantAsync(
                It.IsAny<Ulid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTemperatureCritical_ShouldCreateAlertWithCriticalSeverity()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var tenantId = Ulid.NewUlid();
        var command = new IngestTelemetryReadingCommand(
            cattleId, SensorType.Temperature, 41.5, DateTimeOffset.UtcNow);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = tenantId, TagNumber = "COW-001" }
        }.BuildMockDbSet();

        var alerts = new List<TelemetryAlert>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.TelemetryAlerts).Returns(alerts.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AlertCreated.ShouldBeTrue();
        result.AlertId.ShouldNotBeNull();

        alerts.Verify(
            x => x.Add(It.Is<TelemetryAlert>(a =>
                a.Severity == AlertSeverity.Critical &&
                a.CattleId == cattleId &&
                a.TenantId == tenantId)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenActivityLevelLow_ShouldCreateAlertWithMediumSeverity()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var tenantId = Ulid.NewUlid();
        var command = new IngestTelemetryReadingCommand(
            cattleId, SensorType.ActivityLevel, 15.0, DateTimeOffset.UtcNow);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = tenantId, TagNumber = "COW-002" }
        }.BuildMockDbSet();

        var alerts = new List<TelemetryAlert>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.TelemetryAlerts).Returns(alerts.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AlertCreated.ShouldBeTrue();

        alerts.Verify(
            x => x.Add(It.Is<TelemetryAlert>(a =>
                a.Severity == AlertSeverity.Medium &&
                a.SensorType == SensorType.ActivityLevel)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlertCreated_ShouldCallNotifierWithCorrectTenantAndEventName()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var tenantId = Ulid.NewUlid();
        var command = new IngestTelemetryReadingCommand(
            cattleId, SensorType.Temperature, 40.2, DateTimeOffset.UtcNow);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = tenantId, TagNumber = "COW-003" }
        }.BuildMockDbSet();

        var alerts = new List<TelemetryAlert>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.TelemetryAlerts).Returns(alerts.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x => x.NotifyTenantAsync(
                tenantId,
                "TelemetryAlertCreated",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}