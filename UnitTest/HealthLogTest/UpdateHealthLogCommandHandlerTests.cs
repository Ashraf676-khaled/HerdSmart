// HerdSmart.Tests/Features/HealthLogs/UpdateHealthLogCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.HealthLogs.Commands.UpdateHealthLog;
using HerdSmart.Domain.Entities;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.HealthLogs;

public class UpdateHealthLogCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ILogger<UpdateHealthLogCommandHandler>> _loggerMock;
    private readonly UpdateHealthLogCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public UpdateHealthLogCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _loggerMock = new Mock<ILogger<UpdateHealthLogCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new UpdateHealthLogCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenHealthLogNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateHealthLogCommand(
            Ulid.NewUlid(), "New Diagnosis", "New Treatment", null);

        var healthLogs = new List<HealthLog>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.HealthLogs).Returns(healthLogs.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenHealthLogOlderThan24Hours_ShouldThrowBadRequestException()
    {
        // Arrange
        var logId = Ulid.NewUlid();
        var command = new UpdateHealthLogCommand(
            logId, "New Diagnosis", "New Treatment", null);

        var healthLogs = new List<HealthLog>
        {
            new()
            {
                Id = logId,
                TenantId = _tenantId,
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-25), // ← أكتر من 24 ساعة
                Cattle = new HerdSmart.Domain.Entities.Cattle { TagNumber = "COW-001" }
            }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.HealthLogs).Returns(healthLogs.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateHealthLog()
    {
        // Arrange
        var logId = Ulid.NewUlid();
        var command = new UpdateHealthLogCommand(
            logId, "Updated Diagnosis", "Updated Treatment", "New Notes");

        var existingLog = new HealthLog
        {
            Id = logId,
            TenantId = _tenantId,
            Diagnosis = "Old Diagnosis",
            TreatmentPlan = "Old Treatment",
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1), // ← أقل من 24 ساعة
            Cattle = new HerdSmart.Domain.Entities.Cattle { TagNumber = "COW-001" }
        };

        var healthLogs = new List<HealthLog>
        {
            existingLog
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.HealthLogs).Returns(healthLogs.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        existingLog.Diagnosis.ShouldBe("Updated Diagnosis");
        existingLog.TreatmentPlan.ShouldBe("Updated Treatment");
        existingLog.VetNotes.ShouldBe("New Notes");
        existingLog.UpdatedAt.ShouldNotBeNull();
    }
}