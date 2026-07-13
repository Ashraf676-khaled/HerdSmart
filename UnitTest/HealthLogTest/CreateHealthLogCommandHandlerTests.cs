// HerdSmart.Tests/Features/HealthLogs/CreateHealthLogCommandHandlerTests.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.HealthLog.Commands.CreateHealthLog;
using Application.Features.HealthLogs.Commands.CreateHealthLog;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Shouldly;

namespace HerdSmart.Tests.Features.HealthLogs;

public class CreateHealthLogCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateHealthLogCommandHandler>> _loggerMock;
    private readonly CreateHealthLogCommandHandler _handler;
    private readonly Ulid _tenantId = Ulid.NewUlid();

    public CreateHealthLogCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateHealthLogCommandHandler>>();

        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns(_tenantId);

        _handler = new CreateHealthLogCommandHandler(
            _contextMock.Object,
            _tenantProviderMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCattleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateHealthLogCommand(
            Ulid.NewUlid(), "Fever", "Rest", null, false);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCattleIsDead_ShouldThrowBadRequestException()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new CreateHealthLogCommand(
            cattleId, "Fever", "Rest", null, false);

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            new() { Id = cattleId, TenantId = _tenantId, Status = CattleStatus.Dead }
        }.BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenIsContagious_ShouldIsolateCattle()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new CreateHealthLogCommand(
            cattleId, "FMD", "Isolation", null, true);

        var existingCattle = new HerdSmart.Domain.Entities.Cattle
        {
            Id = cattleId,
            TenantId = _tenantId,
            TagNumber = "COW-001",
            Status = CattleStatus.Active
        };

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            existingCattle
        }.BuildMockDbSet();

        var healthLog = new HerdSmart.Domain.Entities.HealthLog
        {
            CattleId = cattleId,
            TenantId = _tenantId,
            Diagnosis = command.Diagnosis,
            TreatmentPlan = command.TreatmentPlan,
            IsContagious = true
        };

        var healthLogs = new List<HerdSmart.Domain.Entities.HealthLog>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.HealthLogs).Returns(healthLogs.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<HerdSmart.Domain.Entities.HealthLog>(command))
            .Returns(healthLog);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingCattle.Status.ShouldBe(CattleStatus.Isolated);
        result.IsContagious.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenAlreadyIsolated_ShouldNotChangeStatus()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new CreateHealthLogCommand(
            cattleId, "FMD", "Treatment", null, true);

        var existingCattle = new HerdSmart.Domain.Entities.Cattle
        {
            Id = cattleId,
            TenantId = _tenantId,
            TagNumber = "COW-001",
            Status = CattleStatus.Isolated // ← already isolated
        };

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            existingCattle
        }.BuildMockDbSet();

        var healthLog = new HerdSmart.Domain.Entities.HealthLog
        {
            CattleId = cattleId,
            TenantId = _tenantId,
            Diagnosis = command.Diagnosis,
            TreatmentPlan = command.TreatmentPlan,
            IsContagious = true
        };

        var healthLogs = new List<HerdSmart.Domain.Entities.HealthLog>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.HealthLogs).Returns(healthLogs.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<HerdSmart.Domain.Entities.HealthLog>(command))
            .Returns(healthLog);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingCattle.Status.ShouldBe(CattleStatus.Isolated);
        existingCattle.UpdatedAt.ShouldBeNull(); // ← مش اتغير
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateHealthLog()
    {
        // Arrange
        var cattleId = Ulid.NewUlid();
        var command = new CreateHealthLogCommand(
            cattleId, "Fever", "Rest and fluids", "Monitor temperature", false);

        var existingCattle = new HerdSmart.Domain.Entities.Cattle
        {
            Id = cattleId,
            TenantId = _tenantId,
            TagNumber = "COW-001",
            Status = CattleStatus.Active
        };

        var cattle = new List<HerdSmart.Domain.Entities.Cattle>
        {
            existingCattle
        }.BuildMockDbSet();

        var healthLog = new HerdSmart.Domain.Entities.HealthLog
        {
            CattleId = cattleId,
            TenantId = _tenantId,
            Diagnosis = command.Diagnosis,
            TreatmentPlan = command.TreatmentPlan,
            VetNotes = command.VetNotes,
            IsContagious = false
        };

        var healthLogs = new List<HerdSmart.Domain.Entities.HealthLog>()
            .BuildMockDbSet();

        _contextMock.Setup(x => x.Cattle).Returns(cattle.Object);
        _contextMock.Setup(x => x.HealthLogs).Returns(healthLogs.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<HerdSmart.Domain.Entities.HealthLog>(command))
            .Returns(healthLog);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Diagnosis.ShouldBe(command.Diagnosis);
        result.IsContagious.ShouldBeFalse();
        result.CattleTagNumber.ShouldBe("COW-001");
        existingCattle.Status.ShouldBe(CattleStatus.Active);
    }
}